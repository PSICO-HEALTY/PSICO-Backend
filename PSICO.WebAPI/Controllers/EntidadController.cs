using Microsoft.AspNetCore.Mvc;
using PSICO.Core.Interfaces;
using PSICO.WebAPI.Filters;
using PSICO.Data.DTO;

namespace PSICO_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [TenantHeader(true)]
    public class EntidadController : ControllerBase
    {
        private IEntidadService _entidadService;
        public EntidadController(IEntidadService entidadService)
        {
            _entidadService = entidadService;
        }

        [HttpPost]
        public async Task<IActionResult> GestionarEntidad([FromBody] GestionEntidadDto entidad)
        {
            var response = await _entidadService.GestionarEntidad(entidad);

            return Ok(response);
        }
    }
}
