using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace VoxDocs.Configurations
{
    public static class SessionConfig
    {
        public static IServiceCollection AddVoxDocsSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.Name = "VoxDocs.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            return services;
        }

        public static IApplicationBuilder UseVoxDocsSession(this IApplicationBuilder app)
        {
            // Importante: Session deve vir depois de UseRouting() e antes de UseAuthentication()
            app.UseSession();
            return app;
        }
    }
}