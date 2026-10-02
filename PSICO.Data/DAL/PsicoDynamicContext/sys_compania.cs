using System;
using System.Collections.Generic;
using System.Text;

namespace PSICO.Data.DAL.PsicoDynamicContext
{
    public partial class sys_compania
    {
        public string CODIGO { get; set; }
        public string NOMBRE { get; set; }
        public string NOMRES { get; set; }
        public string NIT { get; set; }
        public string DIRECCION { get; set; }
        public string TELEFONO { get; set; }
        public DateTime fechasys { get; set; }
        public string ServerSmtp { get; set; }
        public string CorreoEnvio { get; set; }
        public string PasswordEnvio { get; set; }
        public string EnabledSSL { get; set; }
        public int? puertoSMTP { get; set; }
        public string envia_sms { get; set; }

    }
}
