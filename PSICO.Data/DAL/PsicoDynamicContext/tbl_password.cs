namespace PSICO.Data.DAL.PsicoDynamicContext
{
    public partial class tbl_password
    {
        public string codigoter { get; set; }
        public string password { get; set; }
        public int documento { get; set; }
        public bool FirstLogin { get; set; } // false: aun usa la clave afiliacion; true: ya configuró una clave con hash y seed.
        public string? StringSeed { get; set; }
        public DateTime? LogIngreso { get; set; } // Regista el ultimo instante en que ingreso con el usuario
    }
}
