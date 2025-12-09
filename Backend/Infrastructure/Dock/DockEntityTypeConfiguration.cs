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
                    id => id.AsGuid(),
                    value => new DockId(value))
                .IsRequired();

            builder.OwnsOne(d => d.DockName, dockName =>
            {
                dockName.Property(dn => dn.Value)
                    .HasColumnName("DockName")
                    .HasMaxLength(100)
                    .IsRequired();
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
                            .Select(Guid.Parse).ToList())
                    .HasColumnName("AllowedVesselTypeIds");
            });
        }
    }
}
