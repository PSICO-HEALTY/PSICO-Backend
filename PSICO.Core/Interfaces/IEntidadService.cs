using PSICO.Data.Common;

namespace PSICO.Core.Interfaces
{
    public interface IEntidadService
    {
        Task<bool> ActualizarEntidadesCache();
        AppSettings InitParams(string tenant);
    }
}
