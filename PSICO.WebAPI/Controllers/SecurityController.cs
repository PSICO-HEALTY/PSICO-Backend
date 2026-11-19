using Microsoft.AspNetCore.Mvc;
using PSICO.Core.Interfaces;
using PSICO.Data.DTO;
using PSICO.WebAPI.Filters;
using System.Security.Claims;

namespace PSICO_WebAPI.Controllers
{
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [Route("api/[controller]/[action]/{cc}")]
        [HttpGet]
        public async Task<IActionResult> GetTenant([FromRoute] long cc)
        {
            var tenant = await _securityService.GetTenant(Convert.ToInt64(cc));
            if (tenant is null) return BadRequest("El asociado no existe"); 
            return Ok(tenant);
        }

        [Route("api/[controller]/[action]")]
        [TenantHeader(true)]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUsrDto usr)
        {
            var login = await _securityService.Login(usr.cc, usr.password, usr.tenant, usr.isAdmin);
            if (login is null) return BadRequest("Se ha producido un error inesperado, vuelve a intentarlo.");
            return Ok(login);
        }

        [Route("api/[controller]/[action]")]
        [TenantHeader(true)]
        [HttpPost]
        public async Task<IActionResult> SendToken ([FromBody] SendTokenRequestDto data)
        {
            if(!Enum.IsDefined(typeof(TokenPurpose), data.purpose))
                return BadRequest("Proposito de envio no valido.");

            // Proceso para envio de token sin contexto establecido (recuperacion contraseña)
            if (data.purpose == TokenPurpose.PasswordReset)
            {
                if (string.IsNullOrWhiteSpace(data.cc) ||
                    !int.TryParse(HttpContext.Request.Headers["tenant"].FirstOrDefault(), out var resetTenant))
                    return BadRequest("No fue posible validar la solicitud.");

                await _securityService.SendToken(data.cc, resetTenant, data.option, data.purpose);
            }

            // Proceso para envio de token con contexto extablecido (Login, Cambio de contraseña, etc.)
            if (HttpContext.User.Identity is not ClaimsIdentity identity ||
                !identity.IsAuthenticated ||
                !long.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _) ||
                !int.TryParse(identity.FindFirst("tenant")?.Value, out var tenant))
                return Unauthorized();

            var cc = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await _securityService.SendToken(cc, tenant, data.option, data.purpose);

            if (!result) return BadRequest("Se ha producido un error inesperado, vuelve a intentarlo.");
            return Ok(result);
        }
    }
}
