
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using Infrastructure;

namespace ProjArqsi.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<Dock> Docks { get; set; }
        public DbSet<StorageArea> StorageAreas { get; set; }
        public DbSet<VesselVisitNotification> VesselVisitNotifications { get; set; }
        public DbSet<Container> Containers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VesselTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VesselEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DockEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StorageAreaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VesselVisitNotificationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ContainerEntityTypeConfiguration());
        }
    }

   
}
