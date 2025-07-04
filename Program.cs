using VoxDocs.Configurations;
using VoxDocs.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração básica do host
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
    config.AddEnvironmentVariables();
});

// 2. Configuração do MVC e rotas
builder.Services.AddCustomControllersWithViews();
builder.Services.AddCustomRoutingWithViews();

// 3. Configuração de autenticação
builder.Services.AddVoxDocsAuthentication();

// 4. Configuração de sessão
builder.Services.AddVoxDocsSession();

// 5. Configuração de proteção de dados
builder.Services.AddCustomDataProtectionAndBlobService(builder.Configuration, builder.Environment);

// 6. Configuração do DbContext
builder.Services.AddDbContext<VoxDocsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// 7. Registro de serviços da aplicação
builder.Services.AddApplicationServices();

// 8. Registra o serviço IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 9. Registra o HTTP Cliente
builder.Services.AddHttpClient();

// 10. Registra a memory cache
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ordem CRÍTICA dos middlewares
app.UseRouting();

// Middleware de sessão deve vir ANTES de autenticação/autorização
app.UseVoxDocsSession();

// Middlewares de segurança
app.UseAuthentication();
app.UseAuthorization();

// Configuração de rotas customizadas
app.UseCustomRouting();

app.Run();