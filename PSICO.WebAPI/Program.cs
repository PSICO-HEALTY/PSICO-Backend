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

var myCors = "myCors";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
    var httpContextAccesor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    var tenant = Convert.ToInt32(globalParams.TenantBaseId);
    try { tenant = Convert.ToInt32(httpContextAccesor.HttpContext.Request.Headers["tenant"].First()); }
    catch (Exception ex) { }

    try
    {
        var cache = serviceProvider.GetRequiredService<IMemoryCache>();
        var entidades = (List<EntidadesData>)cache.Get("Entidades");
        var companyData = entidades.FirstOrDefault(x => x.Id == tenant);
    }
    catch (Exception ex) 
    {
        dbContextBuilder.UseSqlServer(psicoDynamicConnectionString);
    }
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
    catch (Exception ex) { tenant = globalParams.TenantBaseId; }

    return entidadService.InitParams(tenant);
});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

//Settings Cache data
var scopeFactory = app.Services.GetService<IServiceScopeFactory>();

using (var scope = scopeFactory.CreateScope())
{
    var provider = scope.ServiceProvider;

    var _entidadService = provider.GetRequiredService<IEntidadService>();

    await _entidadService.ActualizarEntidadesCache();
}

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

app.Run();
