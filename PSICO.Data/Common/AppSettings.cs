namespace PSICO.Data.Common
{
    public class AppSettings
    {
        public int Tenant { get; set; }
        public string Nombre { get; set; }
        public string Solido_Connection_String { get; set; }
        public string Source { get; set; }
        public string Servidor { get; set; }
        public string Puerto { get; set; }
        public string NombreBd { get; set; }
        public string FormaFecha { get; set; }
        public string Hora { get; set; }
        public string Usuario { get; set; }
        public string Pass { get; set; }
        public string Psico_Connection_String_Vb { get; set; }

        public string TipoBd { get; set; }
        public string Dns { get; set; }
        public string FechaHora { get; set; }
        public string SptCodEmpr { get; set; }
        public string PstEmpresa { get; set; }
        public string PstUsuario { get; set; }

        // SMTP Params
        public string ServerSmtp { get; set; }
        public string CorreoEnvio { get; set; }
        public string PasswordEnvio { get; set; }
        public int puertoSMTP { get; set; }
        public bool EnabledSSL { get; set; }
    }
}
