using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace VoxDocs.Configurations
{
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddVoxDocsAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(ConfigureCookieAuthentication);

            return services;
        }

        private static void ConfigureCookieAuthentication(CookieAuthenticationOptions options)
        {
            // Configurações básicas
            options.LoginPath = "/AuthMvc/Login";
            options.AccessDeniedPath = "/AuthMvc/AccessDenied";
            options.LogoutPath = "/AuthMvc/Logout";

            // Configurações de Cookie
            options.Cookie.Name = "VoxDocs.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;

            // Tempo e expiração
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
            options.SlidingExpiration = true;

            // Eventos customizados
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = HandleApiRedirects(StatusCodes.Status401Unauthorized, "Não autenticado"),
                OnRedirectToAccessDenied = HandleApiRedirects(StatusCodes.Status403Forbidden, "Acesso negado"),
                OnRedirectToLogout = HandleLogoutRedirects()
            };
        }

        private static Func<RedirectContext<CookieAuthenticationOptions>, Task> HandleApiRedirects(
            int statusCode, string message) => context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = statusCode;
                    return context.Response.WriteAsJsonAsync(new { Message = message });
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

        private static Func<RedirectContext<CookieAuthenticationOptions>, Task> HandleLogoutRedirects() => context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                return context.Response.WriteAsJsonAsync(new { Message = "Logout realizado com sucesso" });
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        private static bool IsApiRequest(HttpRequest request)
        {
            return request.Path.StartsWithSegments("/api") ||
                   string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}