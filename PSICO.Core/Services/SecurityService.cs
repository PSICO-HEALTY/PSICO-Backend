using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PSICO.Core.Interfaces;
using PSICO.Core.Interfaces.Notificaciones;
using PSICO.Data.Common;
using PSICO.Data.DAL.PsicoDynamicContext;
using PSICO.Data.DAL.PsicoSettingsContext;
using PSICO.Data.DTO;
using PSICO.Data.Interfaces.DAL;
using PSICO.Data.Responses;
using PSICO.Data.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PSICO.Core.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly GlobalParams globalParams;
        private readonly PsicoSettingsDbContext _psicoSettingsContext; // configuraciones del sistema
        private readonly PsicoDynamicSqlDbContext _psicoDynamicSqlDbContext; // Proceso de donde salen los datos
        private readonly ICapaDatos _capaDatos;
        private readonly ISecurityNotificationService _notificationService;
        private readonly IMemoryCache _memoryCache;

        public SecurityService(
            IOptions<GlobalParams> globalParams,
            PsicoSettingsDbContext psicoSettingsContext,
            PsicoDynamicSqlDbContext psicoDynamicSqlDbContext,
            ICapaDatos capaDatos,
            ISecurityNotificationService notificationService,
            IMemoryCache memoryCache)
        {
            this.globalParams = globalParams.Value;
            _psicoSettingsContext = psicoSettingsContext;
            _psicoDynamicSqlDbContext = psicoDynamicSqlDbContext;
            _notificationService = notificationService;
            _capaDatos = capaDatos;
            _memoryCache = memoryCache;
        }

        // Proceso para crear tokens de Autenticacion (Unique)
        private (string, DateTime) GetToken(UserResponse userRsp)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var llave = Encoding.ASCII.GetBytes(globalParams.Secreto);

            var expireToken = DateTime.UtcNow.AddMinutes(globalParams.TokenExpiration);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userRsp.cc),
                    new Claim("Codigoter", userRsp.codigoter),
                    new Claim(ClaimTypes.Email, userRsp.email),
                    new Claim(ClaimTypes.Name, userRsp.nombre),
                    new Claim("IsAdmin", userRsp.isAdmin ? "1" : "0"),
                    new Claim("Tenant", userRsp.tenant),
                    new Claim("TenantId", userRsp.tenantId),
                    new Claim("Cellphone", userRsp.cellphone),
                    new Claim("TenantNombre", userRsp.tenantNombre),
                }),
                Expires = expireToken,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(llave),
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expireToken);
        }

        public async Task<List<TenantResponse>> GetTenant(long cc)
        {
            // Consulta las entidades a las que pertenece el asociado
            var tenants = await (
                from a in _psicoSettingsContext.AsociadosXEntidades
                join b in _psicoSettingsContext.Entidades on a.Entidad_Id equals b.Id
                where a.Asociado_Id == cc
                select new
                {
                    a.Entidad_Id,
                    b.Nombre
                }
                ).ToListAsync();

            // Valida si pertenece a una entidad
            if (tenants == null || !tenants.Any())
            {
                return null!;
            }

            var isAdmin = await _psicoSettingsContext.Asociados
                .Where(x => x.Id == cc)
                .Select(x => x.Is_Admin)
                .FirstOrDefaultAsync();

            // Convertir cc a string para comparación
            var cedula = cc.ToString();

            var tenantResponse = new List<TenantResponse>();

            foreach(var entidad in tenants)
            {
                //Cambia conexion segun tenant
                _capaDatos.ChangeConnectionString(Convert.ToInt32(entidad.Entidad_Id));

                var companySql = @"select l.image from sys_compania c inner join logo l on(c.CODIGO = l.id)";

                var dscompany = _capaDatos.GetDataSet(companySql);

                string companyLogo = string.Empty;
                string companyName = entidad.Nombre;

                //Valida datos
                if (dscompany.Tables.Count > 0 && dscompany.Tables[0].Rows.Count > 0)
                {
                    var row = dscompany.Tables[0].Rows[0];

                    if (row["image"] != DBNull.Value)
                    {
                        byte[] imageBytes = (byte[])row["image"];
                        companyLogo = Convert.ToBase64String(imageBytes);
                    }
                }

                tenantResponse.Add(new TenantResponse
                {
                    tenant = entidad.Entidad_Id.ToString(),
                    isAdmin = isAdmin,
                    nombre = companyName,
                    tenantLogo = companyLogo
                });
            }
            return tenantResponse;
        }

        public async Task<UserResponse> Login(string cc, string password, string tenant, bool isAdmin)
        {
            UserResponse userResponse = null;

            try
            {
                string cedula = cc.PadLeft(14, '0');

                var user = await _psicoDynamicSqlDbContext.tbl_usuario.FirstOrDefaultAsync(
                    x => x.codigoter == cedula
                    );

                var passwordRecord = await _psicoDynamicSqlDbContext.tbl_password
                    .FirstOrDefaultAsync(x => x.codigoter == cedula);

                if (user != null )
                {
                    if (passwordRecord != null && passwordRecord.password == password)
                    {
                        _capaDatos.ChangeConnectionString(Convert.ToInt32(tenant));

                        var companySql = "select c.NOMBRE, l.image from sys_compania c inner join logo l on(c.CODIGO = l.id)";
                        var dscompany = _capaDatos.GetDataSet(companySql);

                        var companyName = string.Empty;
                        var companyLogo = string.Empty;

                        if (dscompany.Tables[0].Rows.Count > 0)
                        {
                            companyName = dscompany.Tables[0].Rows[0]["NOMBRE"].ToString();

                            string base64Encoded = Convert.ToBase64String((byte[])dscompany.Tables[0].Rows[0]["image"]);
                            companyLogo = base64Encoded;
                        }

                        var usrCellphoneSql = $"select MOVIL from tbl_usuario where codigoter='{cedula}'";
                        var dsCellphone = _capaDatos.GetDataSet(usrCellphoneSql);

                        var usrCellphone = string.Empty;

                        if (dsCellphone.Tables[0].Rows.Count > 0)
                            usrCellphone = dsCellphone.Tables[0].Rows[0]["MOVIL"].ToString();

                        userResponse = new UserResponse()
                        {
                            codigoter = user.codigoter,
                            cc = user.cedula,
                            nombre = user.nombre.Trim() + " " + user.apellido.Trim(),
                            email = user.email,
                            isAdmin = isAdmin,
                            tenant = tenant,
                            tenantId = tenant.PadLeft(4, '0'),
                            cellphone = usrCellphone,
                            tenantNombre = companyName,
                            tenantLogo = companyLogo,
                        };


                        //Summary
                        // Futuro uso

                        // Creacion del token para validar ingreso
                        //var token = GetToken(userResponse);

                        //userResponse.token = token.Item1
                        //userResponse.tokenExpire = token.Item2;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception ex) { }

            return userResponse;
        }

        // Envio del token para todos los procesos.
        public async Task<bool> SendToken(string cc, int tenant, int option, TokenPurpose purpose)
        {
            if (!Enum.IsDefined(typeof(TokenPurpose), purpose))
                throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Proposito de token no valido.");

            var response = false;
            if (!long.TryParse(cc, out var asociadoId))
                return false;

            try
            {
                var user = await _psicoSettingsContext.Asociados.FirstOrDefaultAsync(x => x.Id == asociadoId);
                var cedula = cc.PadLeft(14, '0');
                var userData = await _psicoDynamicSqlDbContext.tbl_usuario.FirstOrDefaultAsync(
                    x => x.codigoter == cedula
                    );

                if (user is null || userData is null)
                {
                    return false; //Error
                }

                // Los procesos Contextualizados validan token sencillo; para procesos que cambien clave
                // usan numeros aleatorios seguros para que el codigo no sea facil de predecir.
                var token = purpose == TokenPurpose.Context
                    ? Encrypt.GenerateToken()
                    : System.Security.Cryptography.RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");
                string tokenSeed;
                string tokenHash;

                if (purpose == TokenPurpose.Context) // Procesos Estandard
                {
                    var encryptedToken = Encrypt.StringToEncrypt(token);
                    tokenSeed = encryptedToken.Item1;
                    tokenHash = encryptedToken.Item2;
                }
                else
                {
                    tokenSeed = Encrypt.CreatePasswordTokenSeed(purpose, DateTimeOffset.UtcNow);
                    tokenHash = Encrypt.GetSHA256(tokenSeed + token);
                }

                // Se guardan los datos que permiten comprobar el codigo, no el codigo en texto claro.
                // Usuarios tiene un solo espacio de OTP: este envio invalida cualquier codigo previo
                // de esa persona, incluso si el codigo anterior era para otro proceso.
                user.CadenaRecuperarPass = tokenSeed;
                user.Token = tokenHash;
                await _psicoSettingsContext.SaveChangesAsync();

                response = await _notificationService.SendTokenEmailSms(option, cc, userData.nombre, userData.email, token);
            }
            catch (Exception ex) { }

            return response;
        }
    }
}
