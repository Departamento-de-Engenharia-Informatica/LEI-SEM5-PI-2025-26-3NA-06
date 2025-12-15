using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.StorageArea.ValueObjects;

namespace ProjArqsi.Infrastructure
{
    public class StorageAreaEntityTypeConfiguration : IEntityTypeConfiguration<StorageArea>
    {
        public void Configure(EntityTypeBuilder<StorageArea> builder)
        {
            builder.ToTable("StorageAreas");
            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StorageAreaId(value))
                .IsRequired();

            builder.OwnsOne(sa => sa.Name, name =>
            {
                name.Property(n => n.Value)
                    .HasColumnName("AreaName")
                    .HasMaxLength(100)
                    .IsRequired();
                name.HasIndex(n => n.Value).IsUnique();
            });

            builder.OwnsOne(sa => sa.AreaType, areaType =>
            {
                areaType.Property(at => at.Value)
                    .HasColumnName("AreaType")
                    .HasConversion<int>()
                    .IsRequired();
            });

            builder.OwnsOne(sa => sa.Location, location =>
            {
                location.Property(l => l.Description)
                    .HasColumnName("Location")
                    .HasMaxLength(200)
                    .IsRequired();
            });

            builder.OwnsOne(sa => sa.MaxCapacity, maxCapacity =>
            {
                maxCapacity.Property(mc => mc.Value)
                    .HasColumnName("MaxCapacity")
                    .IsRequired();
            });

            // Do not persist CurrentOccupancy, as it is a calculated value
            builder.Ignore(sa => sa.CurrentOccupancy);

            builder
                .Property(sa => sa.ServedDocks)
                .HasConversion(
                    v => string.Join(',', v.Value.Select(d => d.Value.ToString())),
                    v => new ServedDocks(v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new DockId(Guid.Parse(s))).ToList())
                )
                .HasColumnName("ServedDockIds");

            builder.Ignore(sa => sa.CurrentContainers);
            builder.Property(sa => sa.ServesEntirePort)
                .HasColumnName("ServesEntirePort")
                .IsRequired();
        }
    }
}