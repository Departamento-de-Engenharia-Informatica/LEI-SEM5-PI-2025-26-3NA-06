using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Vessel : Entity<IMOnumber>, IAggregateRoot
    {
        public Guid VesselTypeId { get; private set; }
        public VesselName VesselName { get; private set; }
        public Capacity Capacity { get; private set; }
        public Rows Rows { get; private set; }
        public Bays Bays { get; private set; }
        public Tiers Tiers { get; private set; }
        public Length Length { get; private set; }

        private Vessel()
        {
            VesselName = default!;
            Capacity = default!;
            Rows = default!;
            Bays = default!;
            Tiers = default!;
            Length = default!;
        }

        public Vessel(IMOnumber imo, Guid vesselTypeId, VesselName vesselName, 
                     Capacity capacity, Rows rows, Bays bays, Tiers tiers,
                     Length length)
        {
            Id = imo ?? throw new ArgumentNullException(nameof(imo));
            VesselTypeId = vesselTypeId;
            VesselName = vesselName ?? throw new ArgumentNullException(nameof(vesselName));
            Capacity = capacity ?? throw new ArgumentNullException(nameof(capacity));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
            Bays = bays ?? throw new ArgumentNullException(nameof(bays));
            Tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));
            Length = length ?? throw new ArgumentNullException(nameof(length));
        }

        public void UpdateDetails(VesselName vesselName, Capacity capacity, 
                                 Rows rows, Bays bays, Tiers tiers, Length length)
        {
            VesselName = vesselName;
            Capacity = capacity;
            Rows = rows ;
            Bays = bays ;
            Tiers = tiers ;
            Length = length ;
        }
    }
}
