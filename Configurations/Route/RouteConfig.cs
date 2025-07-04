using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace VoxDocs.Configurations
{
    public static class RouteConfiguration
    {
        public static IServiceCollection AddCustomRoutingWithViews(this IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationFormats.Clear();
                    //Pages
                    options.ViewLocationFormats.Add("/Views/Pages/Index/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Perfil/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Documento/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Upload/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Dashboard/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Pagamento/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Suporte/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/AuthPage/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/AuthPage/RecuperarContas/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/PagamentosPages/{0}.cshtml");

                    //PartialViews/Components/ErrosPages
                    options.ViewLocationFormats.Add("/Views/Shared/ErrosPage/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Shared/Components/SuporteIAVoxDocs/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Shared/Components/Pagamentos/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Shared/Components/Navbar/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Shared/Components/index/{0}.cshtml");
                });

            return services;
        }

        public static IApplicationBuilder UseCustomRouting(this IApplicationBuilder app)
        {
            // Configuração de rotas
            app.UseEndpoints(endpoints =>
            {
                // --- Rota Padrão ---
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // --- Auth Pages ---
                endpoints.MapControllerRoute(
                    name: "Login",
                    pattern: "Login",
                    defaults: new { controller = "AuthMvc", action = "Login" });

                endpoints.MapControllerRoute(
                    name: "ForgotPassword",
                    pattern: "ForgotPassword",
                    defaults: new { controller = "AuthMvc", action = "ForgotPassword" });

                // Intercepta 404 e redireciona internamente
                endpoints.MapFallbackToController("NotFoundPage", "ErrorMvc");
            });

            return app;
        }
    }
}