namespace PSICO.Data.Responses
{
    public class UserResponse
    {
        public string codigoter { get; set; }
        public string cc { get; set; }
        public string nombre { get; set; }
        public string email { get; set; }
        public bool isAdmin { get; set; } = false;
        public string tenant { get; set; }
        public string tenantId { get; set; }
        public string token { get; set; }
        public DateTime tokenExpire { get; set; }
        public string cellphone { get; set; }
        public string tenantNombre { get; set; }
        public string tenantLogo { get; set; }
        /// <summary>False indica que aun debe cambiarse la clave inicial; true indica que ya fue configurada.</summary>
        public bool firstLogin { get; set; }
    }
}
