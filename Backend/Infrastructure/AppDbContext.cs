using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.UserAggregate;
namespace ProjArqsi.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VesselTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        }
    }
}
