using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Infrastructure.SchemaReader;
using GeradorFrontendEnterprise.Infrastructure.ManifestClient;
using GeradorFrontendEnterprise.Infrastructure.TemplateEngine;
using GeradorFrontendEnterprise.Services.Orchestrator;
using GeradorFrontendEnterprise.Services.Generator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Adicionar HttpClient para ManifestClient ANTES de registrar os serviços
builder.Services.AddHttpClient();

// Registrar serviços de infraestrutura
builder.Services.AddScoped<ISchemaReader, SqlServerSchemaReader>();
builder.Services.AddScoped<IManifestClient, HttpManifestClient>();
builder.Services.AddScoped<ITemplateEngine, ScribanTemplateEngine>();

// Registrar serviços de negócio
builder.Services.AddScoped<IGeneratorService, GeneratorService>();
builder.Services.AddScoped<IOrchestratorService, OrchestratorService>();

// Adicionar logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Adicionar sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar sessão
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
