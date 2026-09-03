using Microsoft.EntityFrameworkCore;

namespace PSICO.Data.DAL.PsicoSettingsContext
{
    public partial class PsicoSettingsDbContext : DbContext
    {
        public PsicoSettingsDbContext()
        {
        }

        public PsicoSettingsDbContext(DbContextOptions<PsicoSettingsDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AsociadosXEntidades> AsociadosXEntidades { get; set; }
        public virtual DbSet<Asociados> Asociados { get; set; }
        public virtual DbSet<Entidades> Entidades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asociados>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CadenaRecuperarPass).HasMaxLength(15);
            });

            modelBuilder.Entity<AsociadosXEntidades>(entity =>
            {
                entity.HasOne(d => d.Asociado)
                    .WithMany(p => p.AsociadosXEntidades)
                    .HasForeignKey(d => d.Asociado_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AsocioadosXEntidades_Asociados");

                entity.HasOne(d => d.Entidad)
                    .WithMany(p => p.AsociadosXEntidades)
                    .HasForeignKey(d => d.Entidad_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AsocioadosXEntidades_Entidades");
            });

            modelBuilder.Entity<Entidades>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FormaFecha).IsRequired();

                entity.Property(e => e.Hora).IsRequired();

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.NombreBd).IsRequired();

                entity.Property(e => e.Pass).IsRequired();

                entity.Property(e => e.Puerto).IsRequired();

                entity.Property(e => e.Servidor).IsRequired();

                entity.Property(e => e.Psico_Connection_String_Vb).IsRequired();

                entity.Property(e => e.Source).HasMaxLength(50);

                entity.Property(e => e.StringSeed)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.Usuario).IsRequired();
            });
        }
    }
}
