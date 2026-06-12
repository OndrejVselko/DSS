using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class SimulationDbContext : DbContext
    {
        public DbSet<SimulationRecord> SimulationRecords { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Directory.CreateDirectory("db");
            options.UseSqlite("Data Source=db/simulations.db");
        }

        public static void EnsureCreated()
        {
            using var context = new SimulationDbContext();
            context.Database.EnsureCreated();
        }
    }
}