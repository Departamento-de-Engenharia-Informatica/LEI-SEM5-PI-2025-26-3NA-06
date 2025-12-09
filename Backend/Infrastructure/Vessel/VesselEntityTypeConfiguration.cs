using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.VesselAggregate;

namespace ProjArqsi.Infrastructure
{
    public class VesselEntityTypeConfiguration : IEntityTypeConfiguration<Vessel>
    {
        public void Configure(EntityTypeBuilder<Vessel> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                .HasConversion(
                    id => id.AsString(),
                    str => new IMOnumber(str)
                );

            builder.Property(v => v.VesselTypeId)
                .HasColumnName("VesselTypeId")
                .IsRequired();

            builder.OwnsOne(v => v.VesselName, vn =>
            {
                vn.Property(p => p.Name).HasColumnName("VesselName").IsRequired();
            });

            builder.OwnsOne(v => v.Capacity, c =>
            {
                c.Property(p => p.Value).HasColumnName("Capacity").IsRequired();
            });

            builder.OwnsOne(v => v.Rows, r =>
            {
                r.Property(p => p.Value).HasColumnName("Rows").IsRequired();
            });

            builder.OwnsOne(v => v.Bays, b =>
            {
                b.Property(p => p.Value).HasColumnName("Bays").IsRequired();
            });

            builder.OwnsOne(v => v.Tiers, t =>
            {
                t.Property(p => p.Value).HasColumnName("Tiers").IsRequired();
            });

            builder.OwnsOne(v => v.Length, l =>
            {
                l.Property(p => p.Value).HasColumnName("Length").IsRequired();
            });

            builder.ToTable("Vessels");
        }
    }
}
