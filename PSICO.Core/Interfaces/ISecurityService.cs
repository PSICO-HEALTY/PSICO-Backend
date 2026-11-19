using PSICO.Data.Responses;
using PSICO.Data.DTO;

namespace PSICO.Core.Interfaces
{
    public interface ISecurityService
    {
        Task<List<TenantResponse>> GetTenant(long cc);
        Task<UserResponse> Login(string cc, string password, string tenant, bool isAdmin);
        Task<bool> SendToken(string cc, int tenant, int option, TokenPurpose purpose);
    }
}
