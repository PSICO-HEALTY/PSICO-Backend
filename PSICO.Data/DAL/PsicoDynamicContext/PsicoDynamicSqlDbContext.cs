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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<actualiza_datos>(entity =>
            {
                entity.Property(e => e.Codigoter)
                .HasMaxLength(14)
                .IsUnicode(false);
            }
            );

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
