using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.VesselTypeAggregate;

namespace ProjArqsi.Infrastructure
{
    public class VesselTypeEntityTypeConfiguration : IEntityTypeConfiguration<VesselType>
    {
        public void Configure(EntityTypeBuilder<VesselType> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                .HasConversion(
                    id => id.Value,
                    guid => new VesselTypeId(guid)
                )
                .ValueGeneratedNever();

            builder.OwnsOne(v => v.TypeName, tn =>
            {
                tn.Property(p => p.Value).HasColumnName("TypeName").IsRequired();
            });

            builder.OwnsOne(v => v.TypeDescription, td =>
            {
                td.Property(p => p.Value).HasColumnName("TypeDescription").IsRequired();
            });

            builder.OwnsOne(v => v.TypeCapacity, tc =>
            {
                tc.Property(p => p.Value).HasColumnName("TypeCapacity").IsRequired();
            });

            builder.OwnsOne(v => v.MaxRows, mr =>
            {
                mr.Property(p => p.Value).HasColumnName("MaxRows").IsRequired();
            });

            builder.OwnsOne(v => v.MaxBays, mb =>
            {
                mb.Property(p => p.Value).HasColumnName("MaxBays").IsRequired();
            });

            builder.OwnsOne(v => v.MaxTiers, mt =>
            {
                mt.Property(p => p.Value).HasColumnName("MaxTiers").IsRequired();
            });

            builder.ToTable("VesselTypes");
        }
    }
}
