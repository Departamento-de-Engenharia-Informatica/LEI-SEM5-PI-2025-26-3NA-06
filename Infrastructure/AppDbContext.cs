using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselTypeAggregate;

namespace ProjArqsi.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<VesselType> VesselTypes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VesselTypeEntityTypeConfiguration());
        }
    }
}
