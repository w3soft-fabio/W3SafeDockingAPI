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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
