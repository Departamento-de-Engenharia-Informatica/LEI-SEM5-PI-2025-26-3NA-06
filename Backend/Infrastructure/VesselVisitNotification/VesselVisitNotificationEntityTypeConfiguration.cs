using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.DockAggregate;

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
                    new ValueConverter<ArrivalDate?, DateTime?>(
                        date => date == null ? null : date.Value,
                        value => value == null ? null : new ArrivalDate(value)
                    )
                )
                .IsRequired(false);

            builder.Property(vvn => vvn.DepartureDate)
                .HasConversion(
                    new ValueConverter<DepartureDate?, DateTime?>(
                        date => date == null ? null : date.Value,
                        value => value == null ? null : new DepartureDate(value)
                    )
                )
                .IsRequired(false);

            builder.Property(vvn => vvn.IsHazardous)
                .HasColumnName("IsHazardous")
                .IsRequired();
            
            builder.OwnsOne(vvn => vvn.RejectionReason, rr =>
            {
                rr.Property(r => r.Value).HasColumnName("RejectionReason").IsRequired(false);
            }).Navigation(vvn => vvn.RejectionReason).IsRequired(false);

            // Map TempAssignedDockId as nullable Guid
            builder.Property(vvn => vvn.TempAssignedDockId)
                .HasConversion(
                    id => id != null ? id.AsGuid() : (Guid?)null,
                    value => value.HasValue ? new TempAssignedDockId(value.Value) : null
                )
                .HasColumnName("TempAssignedDockId")
                .IsRequired(false);

            // Ignore the Status navigation property, only map StatusValue
            builder.Ignore(vvn => vvn.Status);
            
            // Map StatusValue as the backing field for Status
            builder.Property(vvn => vvn.StatusValue)
                .HasColumnName("Status")
                .IsRequired();

            // Configure CargoManifests collection (OwnsMany)
            builder.OwnsMany(vvn => vvn.CargoManifests, manifest =>
            {
                manifest.ToTable("CargoManifests");
                
                manifest.WithOwner().HasForeignKey("VesselVisitNotificationId");
                
                manifest.Property<Guid>("VesselVisitNotificationId").IsRequired();
                manifest.Property<int>("Id").ValueGeneratedOnAdd();
                manifest.HasKey("Id");

                // Map ManifestType enum
                manifest.Property(m => m.ManifestType)
                    .HasConversion(
                        mt => (int)mt.Value,
                        value => new ManifestType((ManifestTypeEnum)value)
                    )
                    .HasColumnName("ManifestType")
                    .IsRequired();

                // Configure ManifestEntries collection (OwnsMany within OwnsMany)
                manifest.OwnsMany(m => m.Entries, entry =>
                {
                    entry.ToTable("ManifestEntries");
                    
                    entry.WithOwner().HasForeignKey("CargoManifestId");
                    
                    entry.Property<int>("CargoManifestId").IsRequired();
                    entry.Property<int>("Id").ValueGeneratedOnAdd();
                    entry.HasKey("Id");

                    // Map ContainerId as Guid
                    entry.Property(e => e.ContainerId)
                        .HasConversion(
                            id => id.AsGuid(),
                            value => new ContainerId(value)
                        )
                        .HasColumnName("ContainerId")
                        .IsRequired();

                    // Map SourceStorageAreaId as nullable Guid
                    entry.Property(e => e.SourceStorageAreaId)
                        .HasConversion(
                            id => id != null ? id.AsGuid() : (Guid?)null,
                            value => value.HasValue ? new StorageAreaId(value.Value) : null
                        )
                        .HasColumnName("SourceStorageAreaId")
                        .IsRequired(false);

                    // Map TargetStorageAreaId as nullable Guid
                    entry.Property(e => e.TargetStorageAreaId)
                        .HasConversion(
                            id => id != null ? id.AsGuid() : (Guid?)null,
                            value => value.HasValue ? new StorageAreaId(value.Value) : null
                        )
                        .HasColumnName("TargetStorageAreaId")
                        .IsRequired(false);

                    // Add index on ContainerId for faster lookups
                    entry.HasIndex("ContainerId");
                });
            });

            // Add indexes for common queries
            builder.HasIndex(vvn => vvn.StatusValue).HasDatabaseName("IX_VVN_Status");
            builder.HasIndex(vvn => vvn.ReferredVesselId).HasDatabaseName("IX_VVN_VesselId");
        }
    }
}
