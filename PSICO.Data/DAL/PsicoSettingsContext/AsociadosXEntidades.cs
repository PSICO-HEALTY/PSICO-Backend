namespace PSICO.Data.DAL.PsicoSettingsContext
{
    public partial class AsociadosXEntidades
    {
        public int Id { get; set; }
        public long Asociado_Id { get; set; }
        public int Entidad_Id { get; set; }

        public virtual Asociados Asociado { get; set; }
        public virtual Entidades Entidad { get; set; }
    }
}
