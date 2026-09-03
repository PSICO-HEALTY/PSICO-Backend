using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PSICO.Data.Common;
using PSICO.Data.DAL.PsicoSettingsContext;

namespace PSICO_WebAPI.Middlewares
{
    /// <summary>
    /// Resuelve un tenant por defecto para endpoints públicos previos al login multitenant
    /// (ej. "Olvidé mi contraseña"), donde el asociado todavía no ha seleccionado una entidad.
    ///
    /// Conserva el comportamiento histórico: si el documento pertenece a una o varias entidades,
    /// se utiliza la PRIMERA entidad válida (aplicando el mismo filtro que SecurityService.GetTenant).
    ///
    /// Solo actúa cuando la cabecera 'tenant' está ausente o es inválida, de modo que una selección
    /// explícita de entidad (login normal / peticiones autenticadas) NUNCA se sobrescribe.
    ///
    /// Importante: este middleware solo consulta la base de datos de settings (independiente del
    /// tenant). No debe resolver servicios acoplados a la cabecera, porque dichos contextos se construyen leyendo la cabecera y deben
    /// DESPUÉS de que este middleware la haya fijado.
    /// </summary>
    public class DefaultTenantResolverMiddleware
    {
        private const string TenantHeaderName = "tenant";

        // Rutas públicas (en minúscula) que pueden ejecutarse antes de una selección explícita de entidad.
        private static readonly string[] PreLoginPaths =
        {
            "/api/security/sendpassword/"
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<DefaultTenantResolverMiddleware> _logger;
        
        public DefaultTenantResolverMiddleware(
            RequestDelegate next,
            ILogger<DefaultTenantResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext context,
            PsicoSettingsDbContext settingsContext,
            IOptions<GlobalParams> globalParams)
        {
            try
            {
                if (RequiresDefaultTenant(context))
                {
                    var documento = GetDocumentoFromRoute(context);

                    if (!string.IsNullOrWhiteSpace(documento) && long.TryParse(documento, out var cc))
                    {
                        var gp = globalParams.Value;
                        var (tenantResuelto, cantidadEntidades) = await ResolveDefaultTenantAsync(settingsContext, gp, cc);

                        // Siempre se fija un valor para que la validación de cabecera obligatoria
                        // pase y los contextos acoplados al tenant resuelvan de forma consistente.
                        var tenantAplicado = tenantResuelto?.ToString() ?? gp.TenantBaseId;

                        context.Request.Headers[TenantHeaderName] = tenantAplicado;


                        _logger.LogInformation(
                            "DefaultTenantResolver | documento={Documento} | entidadesEncontradas={Entidades} | tenantResuelto={TenantResuelto} | tenantAplicado={TenantAplicado}",
                            cc,
                            cantidadEntidades,
                            tenantResuelto?.ToString() ?? "(ninguno)",
                            tenantAplicado);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "DefaultTenantResolver | no se pudo determinar el documento desde la ruta '{Path}'.",
                            context.Request.Path.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                // Nunca bloquear el pipeline por un fallo en la resolución del tenant por defecto.
                _logger.LogError(ex, "DefaultTenantResolver | error resolviendo el tenant por defecto.");
            }

            await _next(context);
        }

        private static bool RequiresDefaultTenant(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            if (string.IsNullOrEmpty(path)) return false;
            if (!PreLoginPaths.Any(p => path.Contains(p))) return false;

            // Solo resolver cuando NO existe ya un tenant explícito y válido.
            var current = context.Request.Headers[TenantHeaderName].FirstOrDefault();
            return !IsValidTenant(current);
        }

        private static bool IsValidTenant(string value)
            => !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var id) && id > 0;

        private static string GetDocumentoFromRoute(HttpContext context)
        {
            if (context.Request.RouteValues.TryGetValue("cc", out var ccValue) && ccValue is not null)
            {
                return ccValue.ToString();
            }

            // Respaldo: último segmento de la ruta (api/Security/SendPassword/{cc}).
            var segments = context.Request.Path.Value?.Trim('/').Split('/');
            return segments is { Length: > 0 } ? segments[^1] : null;
        }

        /// <summary>
        /// Devuelve el id de la primera entidad válida asociada al documento y la cantidad total
        /// de entidades encontradas. Aplica el mismo filtro histórico que SecurityService.GetTenant:
        /// se omite la entidad base/de pruebas (TenantBaseId) salvo que el documento esté en una lista blanca a futuro.
        /// </summary>
        private static async Task<(int? tenant, int totalEntidades)> ResolveDefaultTenantAsync(
            PsicoSettingsDbContext settingsContext,
            GlobalParams gp,
            long cc)
        {
            var entidadIds = await (
                from ea in settingsContext.AsociadosXEntidades
                join e in settingsContext.Entidades on ea.Entidad_Id equals e.Id
                where ea.Asociado_Id == cc
                orderby ea.Entidad_Id
                select ea.Entidad_Id
            ).ToListAsync();

            if (entidadIds.Count == 0)
            {
                return (null, 0);
            }

            var cedula = cc.ToString();
            var baseId = int.TryParse(gp.TenantBaseId, out var parsedBase) ? parsedBase : 1;
            //var isWhiteListed = gp.UsersWhiteList != null && gp.UsersWhiteList.Contains(cedula);

            foreach (var id in entidadIds)
            {
                // Conserva la lógica histórica: la entidad base/de pruebas se omite para usuarios no autorizados.
                //if (id == baseId && !isWhiteListed) continue;

                //return (id, entidadIds.Count);
            }

            return (null, entidadIds.Count);
        }
    }
}
