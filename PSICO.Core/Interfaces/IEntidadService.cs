using PSICO.Data.Common;
using PSICO.Data.DTO;
using PSICO.Data.Responses;

namespace PSICO.Core.Interfaces
{
    public interface IEntidadService
    {
        Task<Response> GestionarEntidad(GestionEntidadDto entidad);
        Task<bool> ActualizarEntidadesCache();
        AppSettings InitParams(string tenant);
    }
}
