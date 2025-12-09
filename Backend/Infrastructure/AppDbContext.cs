using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.DockAggregate;

namespace ProjArqsi.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<Dock> Docks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VesselTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VesselEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DockEntityTypeConfiguration());
        }
    }

   
}
