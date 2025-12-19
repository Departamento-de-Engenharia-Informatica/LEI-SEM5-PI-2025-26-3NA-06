using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Infrastructure
{
    public class ContainerEntityTypeConfiguration : IEntityTypeConfiguration<Container>
    {
        public void Configure(EntityTypeBuilder<Container> builder)
        {
            // Map ContainerId value object to Guid
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                .HasConversion(
                    id => id.AsGuid(),
                    value => new ContainerId(value)
                )
                .ValueGeneratedNever()
                .HasColumnName("Id");

            // Map IsoCode as natural key with unique constraint
            builder.OwnsOne(c => c.IsoCode, iso =>
            {
                iso.Property(i => i.Value)
                    .HasColumnName("IsoCode")
                    .IsRequired();
                
                iso.HasIndex(i => i.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Container_IsoCode_Unique");
            });

            // IsHazardous as simple boolean
            builder.Property(c => c.IsHazardous)
                .HasColumnName("IsHazardous")
                .IsRequired();

            // Map CargoType value object
            builder.OwnsOne(c => c.CargoType, ct =>
            {
                ct.Property(p => p.Type).HasColumnName("CargoType").IsRequired();
            });

            // Map ContainerDescription value object
            builder.OwnsOne(c => c.Description, desc =>
            {
                desc.Property(p => p.Text).HasColumnName("Description").IsRequired();
            });

            builder.ToTable("Containers");
        }
    }
}
