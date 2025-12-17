using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Vessel : Entity<IMOnumber>, IAggregateRoot
    {
        public Guid VesselTypeId { get; private set; }
        public VesselName VesselName { get; private set; } = null!;
        public Capacity Capacity { get; private set; } = null!;
        public Rows Rows { get; private set; } = null!;
        public Bays Bays { get; private set; } = null!;
        public Tiers Tiers { get; private set; } = null!;
        public Length Length { get; private set; } = null!;

        private Vessel()
        { }

        public Vessel(IMOnumber imo, Guid vesselTypeId, VesselName vesselName, 
                     Capacity capacity, Rows rows, Bays bays, Tiers tiers,
                     Length length)
        {
            Id = imo;
            VesselTypeId = vesselTypeId;
            VesselName = vesselName;
            Capacity = capacity;
            Rows = rows;
            Bays = bays;
            Tiers = tiers;
            Length = length;
        }
    
        public void UpdateDetails(VesselName vesselName, Capacity capacity, 
                                 Rows rows, Bays bays, Tiers tiers, Length length)
        {
            VesselName = vesselName;
            Capacity = capacity;
            Rows = rows;
            Bays = bays;
            Tiers = tiers;
            Length = length;
        }
    }
}
