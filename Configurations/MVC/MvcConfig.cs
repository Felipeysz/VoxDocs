using Microsoft.AspNetCore.Mvc;

namespace VoxDocs.Configurations
{
    public static class MvcConfiguration
    {
        public static IServiceCollection AddCustomControllersWithViews(this IServiceCollection services)
        {
            // Adiciona os controllers com views padrão
            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation(); // Ativa a compilação de Razor em tempo de execução

            return services;
        }
    }
}
