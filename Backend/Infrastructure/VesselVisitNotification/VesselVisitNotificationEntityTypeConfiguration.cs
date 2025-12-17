using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Infrastructure
{
    public class VesselVisitNotificationEntityTypeConfiguration : IEntityTypeConfiguration<VesselVisitNotification>
    {
        public void Configure(EntityTypeBuilder<VesselVisitNotification> builder) {
                    
                    builder.Property(vvn => vvn.Id)
                        .HasConversion(
                            id => id.AsGuid(), // store as Guid
                            value => new VesselVisitNotificationId(value) // rehydrate VO
                        )
                        .IsRequired();
        
            builder.Property(vvn => vvn.ReferredVesselId)
                .HasConversion(
                    id => id.VesselId.Value, // store as string (IMO)
                    value => new ReferredVesselId(new IMOnumber(value)) // rehydrate VO
                )
                .IsRequired();

            builder.Property(vvn => vvn.ArrivalDate)
                .HasConversion(
                    date => date.Value, // store as DateTime?
                    value => new ArrivalDate(value) // rehydrate VO
                )
                .IsRequired();

            builder.Property(vvn => vvn.DepartureDate)
                .HasConversion(
                    date => date.Value, // store as DateTime?
                    value => new DepartureDate(value) // rehydrate VO
                )
                .IsRequired();
            
            builder.OwnsOne(vvn => vvn.RejectionReason, rr =>
            {
                rr.Property(r => r.Value).HasColumnName("RejectionReason").IsRequired(false);
            });

            // Ignore the Status navigation property, only map StatusValue
            builder.Ignore(vvn => vvn.Status);
            
            // Map StatusValue as the backing field for Status
            builder.Property(vvn => vvn.StatusValue)
                .HasColumnName("Status")
                .IsRequired();
        }
    }
}
