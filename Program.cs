using Microsoft.EntityFrameworkCore;
using VoxDocs.Configurations;
using VoxDocs.Data;
using VoxDocs.Services;
using VoxDocs.BusinessRules;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Storage.Blobs;
using VoxDocs.PagamentosBusinessRules;

var builder = WebApplication.CreateBuilder(args);

// --- Limpa o cookie de autenticação VoxDocsAuthCookie no modo Development ---
if (builder.Environment.IsDevelopment())
{
    // Remove a pasta de chaves de Data Protection para invalidar todos os cookies de autenticação
    var keysFolder = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
    if (Directory.Exists(keysFolder))
    {
        Directory.Delete(keysFolder, true);
    }
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("PermissionAccount", "admin"));
});

// --- Logging & Application Insights ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole().AddDebug();
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// --- DataProtection com fallback ---
builder.Services.AddCustomDataProtection(builder.Configuration);

// Injeção do BlobService Client
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var conn   = config["AzureBlobStorage:ConnectionString"];
    if (string.IsNullOrEmpty(conn))
        throw new InvalidOperationException("AzureBlobStorage:ConnectionString faltando!");
    return new BlobServiceClient(conn);
});


// --- EF Core SQL Server ---
builder.Services.AddDbContext<VoxDocsContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("ConnectionBddVoxDocs"),
        sql => sql.EnableRetryOnFailure()
    )
);

// --- Autenticação via Cookie (usando nossa extensão) ---
builder.Services.AddAuthenticationConfig(builder.Configuration);

// --- Políticas de autorização ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PagePolicy", policy =>
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
    options.AddPolicy("ApiPolicy", policy =>
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
});

// --- Swagger & HttpClient ---
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHttpClient("VoxDocsApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? string.Empty)
);

// --- Serviços ---
builder.Services.AddScoped<IPastaPrincipalService, PastaPrincipalService>();
builder.Services.AddScoped<ISubPastaService, SubPastaService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();




builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlanosVoxDocsService, PlanosVoxDocsService>();
builder.Services.AddScoped<IEmpresasContratanteService, EmpresasContratanteService>();
builder.Services.AddScoped<IPagamentoCartaoFalsoService, PagamentoCartaoFalsoService>();
builder.Services.AddScoped<IPagamentoPixFalsoService, PagamentoPixFalsoService>();
builder.Services.AddScoped<IPagamentoConcluidoService, PagamentoConcluidoService>();


// --- Azure Blob Storage Service ---
builder.Services.AddScoped<AzureBlobService>();

// Regra de Negocios
builder.Services.AddScoped<PagamentosPixBusinessRules>();
builder.Services.AddScoped<PastaPrincipalBusinessRules>();
builder.Services.AddScoped<SubPastaBusinessRules>();
builder.Services.AddScoped<UserBusinessRules>();
builder.Services.AddScoped<EmpresasContratanteRules>();
builder.Services.AddScoped<PlanosVoxDocsRules>();
builder.Services.AddScoped<DocumentoBusinessRules>();

// --- Controllers + Views + Localizações customizadas + Session ---
builder.Services.AddCustomControllersWithViews();      // caso contenha outras configs MVC
builder.Services.AddCustomRoutingWithViews();          // configura localização das Views
builder.Services.AddCustomSession();

var app = builder.Build();

// --- Pipeline padrão ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// --- Swagger UI ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoxDocs API v1");
    c.RoutePrefix = "swagger";
});

// --- Rotas customizadas MVC + Erros + API (se tiver) ---
app.UseCustomRouting();

app.Run();