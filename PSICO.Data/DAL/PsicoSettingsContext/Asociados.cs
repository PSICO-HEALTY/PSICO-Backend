namespace PSICO.Data.DAL.PsicoSettingsContext
{
    public partial class Asociados
    {
        public Asociados() 
        {
            AsociadosXEntidades = new HashSet<AsociadosXEntidades>();
        }

        public long Id { get; set; }
        public bool Is_Admin { get; set; }
        public string Token { get; set; }
        public string CadenaRecuperarPass { get; set; }
        public bool? IsTokenValidado { get; set; }

        public virtual ICollection<AsociadosXEntidades> AsociadosXEntidades {  get; set; }
    }
}
