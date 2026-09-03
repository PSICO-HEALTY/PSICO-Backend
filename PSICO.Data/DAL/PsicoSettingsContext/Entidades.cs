namespace PSICO.Data.DAL.PsicoSettingsContext
{
    public partial class Entidades
    {
        public Entidades()
        {
            AsociadosXEntidades = new HashSet<AsociadosXEntidades>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Psico_Connection_String { get; set; }
        public string Source { get; set; }
        public string StringSeed { get; set; }
        public string Servidor { get; set; }
        public string Puerto { get; set; }
        public string NombreBd { get; set; }
        public string FormaFecha { get; set; }
        public string Hora { get; set; }
        public string Usuario { get; set; }
        public string Pass { get; set; }
        public string Psico_Connection_String_Vb { get; set; }

        public virtual ICollection<AsociadosXEntidades> AsociadosXEntidades { get; set; }
    }
}
