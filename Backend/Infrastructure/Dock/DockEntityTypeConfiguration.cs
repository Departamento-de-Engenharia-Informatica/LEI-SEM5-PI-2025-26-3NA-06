using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.DockAggregate;

namespace ProjArqsi.Infrastructure
{
    internal class DockEntityTypeConfiguration : IEntityTypeConfiguration<Dock>
    {
        public void Configure(EntityTypeBuilder<Dock> builder)
        {
            builder.ToTable("Docks");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasConversion(
                    id => id.Value,
                    value => new DockId(value))
                .ValueGeneratedNever()
                .IsRequired();

            builder.OwnsOne(d => d.DockName, dockName =>
            {
                dockName.Property(dn => dn.Value)
                    .HasColumnName("DockName")
                    .HasMaxLength(100)
                    .IsRequired();
                dockName.HasIndex(dn => dn.Value).IsUnique();
            });

            builder.OwnsOne(d => d.Location, location =>
            {
                location.Property(l => l.Description)
                    .HasColumnName("LocationDescription")
                    .HasMaxLength(500)
                    .IsRequired();
            });

            builder.OwnsOne(d => d.Length, length =>
            {
                length.Property(l => l.Value)
                    .HasColumnName("Length")
                    .IsRequired();
            });

            builder.OwnsOne(d => d.Depth, depth =>
            {
                depth.Property(d => d.Value)
                    .HasColumnName("Depth")
                    .IsRequired();
            });

            builder.OwnsOne(d => d.MaxDraft, maxDraft =>
            {
                maxDraft.Property(md => md.Value)
                    .HasColumnName("MaxDraft")
                    .IsRequired();
            });

            builder.OwnsOne(d => d.AllowedVesselTypes, allowedVesselTypes =>
            {
                allowedVesselTypes.Property(avt => avt.VesselTypeIds)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Guid.Parse).ToList(),
                        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Guid>>(
                            (c1, c2) => c1!.SequenceEqual(c2!),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()))
                    .HasColumnName("AllowedVesselTypeIds");
            });
        }
    }
}
