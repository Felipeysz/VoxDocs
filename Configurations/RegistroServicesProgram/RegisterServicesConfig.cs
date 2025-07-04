using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;
using VoxDocs.BusinessRules;
using VoxDocs.Models;
using VoxDocs.Services.Validators;
using VoxDocs.Validators;

namespace VoxDocs.Configurations
{
    public static class RegisterServicesConfig
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Registro automático de validadores FluentValidation
            services.AddValidatorsFromAssemblyContaining<UserValidation>();
            
            // 2. Registro manual de validadores customizados
            services.AddCustomValidators();
            
            // 3. Registro automático por convenção
            services.RegisterByConvention();
            
            return services;
        }

        private static IServiceCollection AddCustomValidators(this IServiceCollection services)
        {
            // Validações customizadas que não seguem o padrão FluentValidation
            services.AddScoped<IPastaPrincipalValidator, PastaPrincipalValidator>();
            services.AddScoped<IDocumentoValidator, DocumentoValidator>();
            services.AddScoped<AbstractValidator<DocumentoModel>, DocumentoValidator>();
            services.AddScoped<IPlanosValidator, PlanosValidator>();
            services.AddScoped<IPagamentoValidator, PagamentoValidator>();


            return services;
        }

        private static IServiceCollection RegisterByConvention(this IServiceCollection services)
        {
            services.Scan(scan =>
            {
                scan.FromAssemblies(Assembly.GetExecutingAssembly())
                    // Registra repositórios
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    
                    // Registra serviços
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            return services;
        }
    }
}