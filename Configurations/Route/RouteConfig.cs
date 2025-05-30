namespace VoxDocs.Configurations
{
    public static class RouteConfiguration
    {
        public static void AddCustomRoutingWithViews(this IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("/Views/Pages/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/AuthPage/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/DocumentosPage/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/PagamentosPages/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/AccountPage/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/AdminPages/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Components/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Pages/Components/Documentos/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Views/Error/{0}.cshtml");
                });
        }

        public static void UseCustomRouting(this WebApplication app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

            // Rotas MVC específicas
            app.MapControllerRoute(
            name: "/",
            pattern: "/",
            defaults: new { controller = "IndexMvc", action = "Index" });
            app.MapControllerRoute(
            name: "index",
            pattern: "index",
            defaults: new { controller = "IndexMvc", action = "Index" });


            app.MapControllerRoute(
                name: "Pagamentos",
                pattern: "Pagamentos",
                defaults: new { controller = "PagamentosMvc", action = "Pagamentos" });

            app.MapControllerRoute(
                name: "ConfirmandoPagamentoPix",
                pattern: "ConfirmandoPagamentoPix",
                defaults: new { controller = "PagamentosMvc", action = "ConfirmandoPagamentoPix" });

            app.MapControllerRoute(
                name: "Login",
                pattern: "Login",
                defaults: new { controller = "LoginMvc", action = "Login" });

            app.MapControllerRoute(
                name: "MeuPerfil",
                pattern: "MeuPerfil",
                defaults: new { controller = "UserInfoMvc", action = "MeuPerfil" });

            app.MapControllerRoute(
                name: "DocumentosPagina",
                pattern: "DocumentosPagina",
                defaults: new { controller = "DocumentosPaginaMvc", action = "DocumentosPagina" });
            
            app.MapControllerRoute(
                name: "UploadDocumentos",
                pattern: "UploadDocumentos",
                defaults: new { controller = "DocumentosMvc", action = "UploadDocumentos" });

            app.MapControllerRoute(
                name: "LinkRedefinirSenha",
                pattern: "LinkRedefinirSenha",
                defaults: new { controller = "LoginMvc", action = "LinkRedefinirSenha" });

            app.MapControllerRoute(
                name: "RecuperarSenha",
                pattern: "RecuperarSenha",
                defaults: new { controller = "LoginMvc", action = "RecuperarSenha" });

            app.MapControllerRoute(
                name: "RecuperarEmail",
                pattern: "RecuperarEmail",
                defaults: new { controller = "LoginMvc", action = "RecuperarEmail" });



            //Rotas Administrativas
            app.MapControllerRoute(
                name: "Dashboard",
                pattern: "Dashboard",
                defaults: new { controller = "AdminMvc", action = "Dashboard" });


            // Rotas de erro
            app.MapControllerRoute(
                name: "LoginNotFound",
                pattern: "LoginNotFound",
                defaults: new { controller = "ErrorMvc", action = "LoginNotFound" });

            app.MapControllerRoute(
                name: "SemTokenConfirmandoPagamentoPix",
                pattern: "SemTokenConfirmandoPagamentoPix",
                defaults: new { controller = "ErrorMvc", action = "SemTokenConfirmandoPagamentoPix" });

            app.MapControllerRoute(
                name: "NotFoundPage",
                pattern: "NotFoundPage",
                defaults: new { controller = "ErrorMvc", action = "NotFoundPage" });

            app.MapControllerRoute(
                name: "Navbar",
                pattern: "NavbarMvc/{action=Logout}",
                defaults: new { controller = "NavbarMvc", action = "Logout" });

            app.MapControllerRoute(
                name: "ErrorTokenInvalido",
                pattern: "ErrorTokenInvalido",
                defaults: new { controller = "ErrorMvc", action = "ErrorTokenInvalido" });

            app.MapControllerRoute(
                name: "ErrorOfflinePage",
                pattern: "ErrorOfflinePage",
                defaults: new { controller = "ErrorMvc", action = "ErrorOfflinePage" });


            // Intercepta 404 e redireciona internamente
            app.UseStatusCodePagesWithReExecute("/NotFoundPage");
        }
    }
}