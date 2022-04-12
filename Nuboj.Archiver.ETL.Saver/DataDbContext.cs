using Microsoft.EntityFrameworkCore;
using Nuboj.Archiver.ETL.Saver.Models;

namespace Nuboj.Archiver.ETL.Saver
{
    public class DataDbContext : DbContext
    {
        public DataDbContext()
        {
        }

        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }

        public DbSet<Status> Status { get; set; }
        public DbSet<Nvm> Nvm { get; set; }
        public DbSet<ComponentInfo> ComponentInfo { get; set; }
        public DbSet<JsonFilename> JsonFilenames { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder opt)
        {
            if (!opt.IsConfigured)
            {
                opt.UseNpgsql(GetConnectionString()/*, 
                    o => o.EnableRetryOnFailure()*/);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Models.File>()
            //    .HasNoKey();
            //modelBuilder.Entity<Status>()
            //    .HasNoKey();
            //modelBuilder.Entity<Nvm>()
            //    .HasNoKey();
            //modelBuilder.Entity<ComponentInfo>()
            //    .HasNoKey();

            // Composite PK
            //modelBuilder.Entity<Status>()
            //.HasKey(s => new {s.SensorDataTimestamp, s.})
        }

        protected string GetConnectionString()
        {
            var conn = Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING");
            //var conn = @"Server=127.0.0.1;Port=5432;Database=etl;User Id=saver;Password=saver";
            
            return conn;
        }
    }
}
