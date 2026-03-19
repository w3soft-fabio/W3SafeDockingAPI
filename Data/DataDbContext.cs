using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Data
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }

        public DbSet<Ship> Ships { get; set; }
        public DbSet<BerthingAlarm> BerthingAlarms { get; set; }
        public DbSet<DriftingAlarm> DriftingAlarms { get; set; }
        public DbSet<MooringPattern> MooringPatterns { get; set; }
        public DbSet<AlarmThreshold> AlarmThresholds { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<GrupoRecurso> GrupoRecursos { get; set; }
        public DbSet<GrupoUsuario> GrupoUsuarios { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PrimeiroAcessoToken> PrimeiroAcessoTokens { get; set; }
        public DbSet<BerthSnapshot> BerthSnapshots { get; set; }
        public DbSet<MooringCompany> MooringCompanies { get; set; }
        public DbSet<ShippingAgency> ShippingAgencies { get; set; }
        public DbSet<Berthing> Berthings { get; set; }
        public DbSet<EmailConta> EmailContas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BerthSnapshot>(entity =>
            {
                entity.HasIndex(e => new { e.BercoId, e.CapturedAt })
                      .HasDatabaseName("idx_berco_captured");
            });
        }
    }
}
