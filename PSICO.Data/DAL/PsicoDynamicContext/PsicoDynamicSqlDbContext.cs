using Microsoft.EntityFrameworkCore;

namespace PSICO.Data.DAL.PsicoDynamicContext
{
    public partial class PsicoDynamicSqlDbContext : DbContext
    {
        public PsicoDynamicSqlDbContext()
        { 
        }

        public PsicoDynamicSqlDbContext(DbContextOptions<PsicoDynamicSqlDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<actualiza_datos> actualiza_datos { get; set; }
        public virtual DbSet<sys_compania> sys_compania { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<actualiza_datos>(entity =>
            {
                entity.Property(e => e.Codigoter)
                .HasMaxLength(14)
                .IsUnicode(false);
            }
            );

            modelBuilder.Entity<sys_compania>(entity =>
            {
                //Datos de la entidad
                entity.HasKey(e => e.CODIGO)
                    .HasName("PK__sys_compania__2A164134");

                entity.Property(e => e.CODIGO)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.NIT)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.NOMBRE)
                    .IsRequired()
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.NOMRES)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.TELEFONO)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DIRECCION)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.fechasys)
                    .HasColumnType("smalldatetime")
                    .HasDefaultValueSql("('01-01-1950')");

                // Datos para notificacion SMTP
                entity.Property(e => e.EnabledSSL)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ServerSmtp)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(' ')");

                entity.Property(e => e.PasswordEnvio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(' ')");

                entity.Property(e => e.CorreoEnvio)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(' ')");

                // Datos para notificaciones SMS
                entity.Property(e => e.envia_sms)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('N')");
            });
                OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
