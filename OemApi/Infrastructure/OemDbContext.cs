using Microsoft.EntityFrameworkCore;

namespace ProjArqsi.OemApi.Infrastructure
{
    public class OemDbContext : DbContext
    {
        public OemDbContext(DbContextOptions<OemDbContext> options) : base(options)
        {
        }

        // Future entities will be added here
        // public DbSet<OperationPlan> OperationPlans { get; set; }
        // public DbSet<VesselOperation> VesselOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Entity configurations will be added here
            // modelBuilder.ApplyConfiguration(new OperationPlanConfiguration());
        }
    }
}
