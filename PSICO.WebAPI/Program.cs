using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using PSICO.WebAPI.Filters;
using PSICO_WebAPI.Middlewares;
using System.Globalization;
using System.Text;
using PSICO.Data.Common;
using PSICO.Data.DAL.PsicoSettingsContext;
using Microsoft.EntityFrameworkCore;
using PSICO.Data.DAL.PsicoDynamicContext;
using Microsoft.Extensions.Caching.Memory;
using PSICO.Core.Interfaces;
using PSICO.Core.Services;
using PSICO.Data.DAL;
using PSICO.Data.Interfaces.DAL;

var myCors = "myCors";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICapaDatos, CapaDatos>();
builder.Services.AddScoped<IEntidadService, EntidadService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<TenantHeader>();
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myCors, builder =>
    {
        builder.WithOrigins("*")
            .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Cache Initial Configuration
builder.Services.AddMemoryCache();

// Initial application parameters
var globalParamsSection = builder.Configuration.GetSection("GlobalParams");

// JWT
var globalParams = globalParamsSection.Get<GlobalParams>();
var llave = Encoding.ASCII.GetBytes(globalParams.Secreto);

builder.Services.AddAuthentication(d =>
{
    d.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    d.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(d =>
{
    d.RequireHttpsMetadata = false;
    d.SaveToken = true;
    d.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(llave),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

// Configuring PsicoSettingsDb connection
var psicoSettingsConnectionString = builder.Configuration.GetConnectionString("PsicoSettingsConnection");

builder.Services.AddDbContext<PsicoSettingsDbContext>(opciones =>
    opciones.UseSqlServer(psicoSettingsConnectionString) // ServiceLifetime.Scope
    );

// Configuring Psico Dynamic DB connection using field in the HTTP header
var psicoDynamicConnectionString = builder.Configuration.GetConnectionString("PsicoDynamicConnection");

builder.Services.AddDbContext<PsicoDynamicSqlDbContext>((serviceProvider, dbContextBuilder) =>
{
    var appSettings = serviceProvider.GetRequiredService<AppSettings>();
    var connectionString = appSettings.Psico_Connection_String;

    if (string.IsNullOrWhiteSpace(connectionString) &&
        appSettings.Tenant.ToString() == globalParams.TenantBaseId)
    {
        connectionString = psicoDynamicConnectionString;
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException($"No hay conexión configurada para el tenant {appSettings.Tenant}.");
    }

    dbContextBuilder.UseSqlServer(connectionString);
});

//Static global params
builder.Services.Configure<GlobalParams>(globalParamsSection);

//Dynamic params
builder.Services.AddScoped(e =>
{
    /* Process to load Entity Data Dinamically */

    var httpContextAccesor = e.GetRequiredService<IHttpContextAccessor>();
    var entidadService = e.GetRequiredService<IEntidadService>();

    var tenant = string.Empty;
    try { tenant = httpContextAccesor.HttpContext.Request.Headers["tenant"].First(); }
    catch { tenant = globalParams.TenantBaseId; }

    return entidadService.InitParams(tenant);
});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Resuelve un tenant por defecto (primera entidad válida) para endpoints públicos previos
// al login multitenant cuando la cabecera 'tenant' aún no existe (ej. "Olvidé mi contraseña").
// Debe ejecutarse ANTES del validador de cabecera y antes de que se construyan los contextos
// acoplados al tenant (resueltos al activar el controlador).
app.UseMiddleware<DefaultTenantResolverMiddleware>();

app.UseMiddleware<CustomHeaderValidatorMiddleware>(TenantHeader.HeaderName);

app.UseHttpsRedirection();

app.UseStaticFiles();
//app.UseRouting();

app.UseCors(x =>
    x.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

var supportedCultures = new[]
{
     new CultureInfo("es-CO")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-CO"),

    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,

    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});

await app.StartAsync();

try
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    using var scope = scopeFactory.CreateScope();
    var entidadService = scope.ServiceProvider.GetRequiredService<IEntidadService>();
    var cacheTask = entidadService.ActualizarEntidadesCache();
    var cacheLoaded = await cacheTask.WaitAsync(TimeSpan.FromSeconds(15));

    if (!cacheLoaded)
    {
        Console.WriteLine("No se pudo cargar la cache de entidades.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error cargando la cache de entidades: {ex.Message}");
}

await app.WaitForShutdownAsync();
