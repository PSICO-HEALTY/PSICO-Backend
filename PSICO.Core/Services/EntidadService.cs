using PSICO.Core.Interfaces;
using PSICO.Data.DAL.PsicoSettingsContext;
using Microsoft.Extensions.Caching.Memory;
using PSICO.Data.Common;
using Microsoft.Extensions.Options;
using PSICO.Data.Enumerables;
using PSICO.Data.Responses;
using PSICO.Data.DTO;
using Microsoft.EntityFrameworkCore;
using PSICO.Data.Tools;
using PSICO.Data.DAL.PsicoDynamicContext;
using PSICO.Data.Interfaces.DAL;

namespace PSICO.Core.Services
{
    public class EntidadService : IEntidadService
    {
        private readonly PsicoSettingsDbContext _psicoSettingsContext;
        private readonly PsicoDynamicSqlDbContext _psicoDynamicContext;
        private readonly IMemoryCache _cache;
        private readonly ICapaDatos _capaDatos;

        public EntidadService(
            PsicoSettingsDbContext psicoSettingsContext,
            PsicoDynamicSqlDbContext psicoDynamicContext,
            IMemoryCache cache,
            ICapaDatos capaDatos,
            IOptions<GlobalParams> globalParams)
        {
            _psicoSettingsContext = psicoSettingsContext;
            _psicoDynamicContext = psicoDynamicContext;
            _cache = cache;
            _capaDatos = capaDatos;
        }

        private (bool, string) GetConnectionString(TipoBdEnum tipoBd, string server, string userId, string password, string database = "")
        {
            var result = false;
            var connectionString = string.Empty;

            try
            {
                var sourceDb = string.Empty;

                switch (tipoBd)
                {
                    case TipoBdEnum.Mssql:
                        sourceDb = "sqlsource";
                        connectionString = $"data source={server};initial catalog={database!};user id={userId};password={password};Encrypt=false;TrustServerCertificate=true";
                        break;

                    case TipoBdEnum.Oracle:
                        sourceDb = "oraclesource";
                        connectionString = $"User ID={userId};Password={password};Data Source={server}";
                        break;

                    default:
                        break;
                }

                _capaDatos.ChangeConnectionStringTest(connectionString, sourceDb);

                var sql = "select * from sys_compania";
                var ds = _capaDatos.GetDataSet(sql);
                result = true;
            }
            catch (Exception ex) { }

            return (result, connectionString);
        }

        public async Task<Response> GestionarEntidad(GestionEntidadDto entidad)
        {
            var response = new Response();

            try
            {
                if (!string.IsNullOrEmpty(entidad.nombre.Trim()) && !string.IsNullOrEmpty(entidad.servidor.Trim()) &&
                    !string.IsNullOrEmpty(entidad.puerto.Trim()) && !string.IsNullOrEmpty(entidad.source.Trim()) &&
                    !string.IsNullOrEmpty(entidad.nombreBd.Trim()) && !string.IsNullOrEmpty(entidad.formaFecha.Trim()) &&
                    !string.IsNullOrEmpty(entidad.hora.Trim()) && !string.IsNullOrEmpty(entidad.usuario.Trim()) &&
                    !string.IsNullOrEmpty(entidad.pass.Trim()))
                {
                    var sourceValidation = entidad.source switch
                    {
                        "sqlsource" => (true, TipoBdEnum.Mssql),
                        "oraclesource" => (true, TipoBdEnum.Oracle),
                        _ => (false, TipoBdEnum.Mssql)
                    };

                    if (sourceValidation.Item1)
                    {
                        var testPsicoConnectionString = GetConnectionString(sourceValidation.Item2, entidad.servidor, entidad.usuario, 
                            entidad.pass, entidad.nombreBd);

                        if (testPsicoConnectionString.Item1)
                        {
                            var psicoConnectionString = testPsicoConnectionString.Item2;

                            // Validamos si la entidad esta parametrizada en la BD
                            var entidadBd = await _psicoSettingsContext.Entidades
                                .FirstOrDefaultAsync(x => x.Id == entidad.tenant);

                            // Generamos una llave de encriptacion unica por entidad
                            var stringSeed = Encrypt.GenerateSeed();

                            /* Encriptamos la informacion sensible de la entidad */
                            var servidorEncrypted = Encrypt.EncryptRijndael(entidad.servidor, stringSeed);
                            var puertoEncrypted = Encrypt.EncryptRijndael(entidad.puerto, stringSeed);
                            var sourceEncrypted = Encrypt.EncryptRijndael(entidad.source, stringSeed);
                            var nombreBdEncrypted = Encrypt.EncryptRijndael(entidad.nombreBd, stringSeed);
                            var formaFechaEncrypted = Encrypt.EncryptRijndael(entidad.formaFecha, stringSeed);
                            var horaEncrypted = Encrypt.EncryptRijndael(entidad.hora, stringSeed);
                            var usuarioEncrypted = Encrypt.EncryptRijndael(entidad.usuario, stringSeed);
                            var passEncrypted = Encrypt.EncryptRijndael(entidad.pass, stringSeed);

                            var connectionStringEncrypted = Encrypt.EncryptRijndael(psicoConnectionString, stringSeed);

                            if (entidadBd is null)
                            {
                                
                                // Creamos la entidad para almacenar en la BD centralizada
                                var newEntidad = new Entidades()
                                {
                                    Id = entidad.tenant,
                                    Nombre = entidad.nombre,
                                    Psico_Connection_String = connectionStringEncrypted,
                                    Source = sourceEncrypted,
                                    StringSeed = stringSeed,
                                    Servidor = servidorEncrypted,
                                    Puerto = puertoEncrypted,
                                    NombreBd = nombreBdEncrypted,
                                    FormaFecha = formaFechaEncrypted,
                                    Hora = horaEncrypted,
                                    Usuario = usuarioEncrypted,
                                    Pass = passEncrypted
                                };

                                // Creamos y Guardamos cambios en la BD
                                await _psicoSettingsContext.Entidades.AddAsync(newEntidad);
                                await _psicoSettingsContext.SaveChangesAsync();

                                response.message = "Entidad creada correctamente.";
                            }
                            else
                            {
                                entidadBd.Nombre = entidad.nombre;
                                entidadBd.Psico_Connection_String = connectionStringEncrypted;
                                entidadBd.Source = sourceEncrypted;
                                entidadBd.StringSeed = stringSeed;
                                entidadBd.Servidor = servidorEncrypted;
                                entidadBd.Puerto = puertoEncrypted;
                                entidadBd.NombreBd = nombreBdEncrypted;
                                entidadBd.FormaFecha = formaFechaEncrypted;
                                entidadBd.Hora = horaEncrypted;
                                entidadBd.Usuario = usuarioEncrypted;
                                entidadBd.Pass = passEncrypted;

                                // Actualizamos cambios en la BD
                                await _psicoSettingsContext.SaveChangesAsync();

                                response.message = "Entidad actualizada exitosamente.";
                            }

                            // Actualizacion de la data de Entidades en Cache
                            await ActualizarEntidadesCache();

                            response.result = true;
                        }

                        else
                        {
                            response.message = "Se ha intentado establecer una conexión a la Base de Datos parametrizada " +
                                               "pero no ha sido exitosa. Intentelo de nuevo.";
                        }
                    }

                    else { response.message = "Source incorrecto. Intentelo de nuevo.";}

                }

                else { response.message = "Entidad parametrizada incorrectamente. Intentelo de nuevo.";}
                
            }
            catch (Exception ex) 
            {
                response.message = ex.Message;
            }

            return response;
        }

        private async Task<List<EntidadesData>> GetDataEntidades()
        {
            var entidades = new List<EntidadesData>();

            try
            {
                entidades = await _psicoSettingsContext.Entidades
                            .Select(x => new EntidadesData()
                            {
                                Id = x.Id,
                                Nombre = x.Nombre,
                                Psico_Connection_String = Encrypt.DecryptRijndael(x.Psico_Connection_String, x.StringSeed),
                                Source = Encrypt.DecryptRijndael(x.Source, x.StringSeed),
                                Servidor = Encrypt.DecryptRijndael(x.Servidor, x.StringSeed),
                                Puerto = Encrypt.DecryptRijndael(x.Puerto, x.StringSeed),
                                NombreBd = Encrypt.DecryptRijndael(x.NombreBd, x.StringSeed),
                                FormaFecha = Encrypt.DecryptRijndael(x.FormaFecha, x.StringSeed),
                                Hora = Encrypt.DecryptRijndael(x.Hora, x.StringSeed),
                                Usuario = Encrypt.DecryptRijndael(x.Usuario, x.StringSeed),
                                Pass = Encrypt.DecryptRijndael(x.Pass, x.StringSeed),

                                TipoBd = (Encrypt.DecryptRijndael(x.Source, x.StringSeed) == "sqlsource") ?
                                            "Sql" : "Oracle",

                                Dns = (Encrypt.DecryptRijndael(x.Source, x.StringSeed) == "sqlsource") ?
                                        "sql server" : "oracle",

                                FechaHora = $"{Encrypt.DecryptRijndael(x.FormaFecha, x.StringSeed)} " +
                                            $"{Encrypt.DecryptRijndael(x.Hora, x.StringSeed)}",

                                SptCodEmpr = "0001",
                                PstEmpresa = $"0001 {x.Nombre}",
                                PstUsuario = "admin"

                            }).ToListAsync();
            }

            catch (Exception) { }

            return entidades;
        }

        private void ClearCacheEntidades()
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    // 1.0 --> 100%
                    var percentage = 1.0;

                    // Clear Cache
                    memoryCache.Compact(percentage);
                }
            }

            catch (Exception) { }
        }

        public async Task<bool> ActualizarEntidadesCache()
        {
            var result = false;

            try
            {
                var entidades = await GetDataEntidades();

                if (entidades.Count != 0)
                {
                    // Limpieza del cache
                    ClearCacheEntidades();

                    // Se actualizan los valores de las entidades en cache
                    _cache.Set("Entidades", entidades);

                    result = true;
                }
            }

            catch (Exception) { }

            return result;
        }

        private async Task<sys_compania> GetSmtpParams(int tenant)
            => await _psicoDynamicContext.sys_compania
               .Select(x => new sys_compania()
               {
                   ServerSmtp = x.ServerSmtp,
                   CorreoEnvio = x.CorreoEnvio,
                   PasswordEnvio = x.PasswordEnvio,
                   puertoSMTP = x.puertoSMTP,
                   EnabledSSL = x.EnabledSSL
               }).FirstOrDefaultAsync();

        public AppSettings InitParams(string tenant)
        {
            AppSettings appSettings = null!;

            try 
            { 
                var entidades = (List<EntidadesData>) _cache.Get("Entidades")!;
                var companyData = entidades.FirstOrDefault(x => x.Id == Convert.ToInt32(tenant));

                if (companyData != null)
                {
                    var smtpParams = Task.Run(() =>
                                         GetSmtpParams(Convert.ToInt32(tenant))
                                         ).GetAwaiter().GetResult();

                    appSettings = new AppSettings()
                    {
                        Tenant = companyData.Id,
                        Nombre = companyData.Nombre,
                        Psico_Connection_String = companyData.Psico_Connection_String,
                        Source = companyData.Source,
                        Servidor = companyData.Servidor,
                        Puerto = companyData.Puerto,
                        NombreBd = companyData.NombreBd,
                        FormaFecha = companyData.FormaFecha,
                        Hora = companyData.Hora,
                        Usuario = companyData.Usuario,
                        Pass = companyData.Pass,
                        TipoBd = companyData.TipoBd,
                        Dns = companyData.Dns,
                        FechaHora = companyData.FechaHora,
                        SptCodEmpr = companyData.SptCodEmpr,
                        PstUsuario = companyData.PstUsuario,
                        PstEmpresa = companyData.PstEmpresa,

                        ServerSmtp = smtpParams.ServerSmtp,
                        CorreoEnvio = smtpParams.CorreoEnvio,
                        PasswordEnvio = smtpParams.PasswordEnvio,
                        puertoSMTP = smtpParams.puertoSMTP.GetValueOrDefault(),
                        EnabledSSL = (smtpParams.EnabledSSL.Trim() == "Y")
                    };
                }
            }
            catch (Exception ex) { }

            return appSettings;
        }
    }
}
