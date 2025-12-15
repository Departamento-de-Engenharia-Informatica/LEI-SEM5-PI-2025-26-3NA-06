using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselTypeAggregate;

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
                     Length length, VesselType vesselType)
        {
            if (vesselType == null)
                throw new BusinessRuleValidationException("Vessel type must exist.");

            if (capacity.Value > vesselType.TypeCapacity.Value)
                throw new BusinessRuleValidationException($"Vessel capacity ({capacity.Value}) cannot exceed the vessel type capacity ({vesselType.TypeCapacity.Value}).");

            if (rows.Value > vesselType.MaxRows.Value)
                throw new BusinessRuleValidationException($"Vessel Rows ({rows.Value}) cannot exceed the vessel type max rows ({vesselType.MaxRows.Value}).");

            if (bays.Value > vesselType.MaxBays.Value)
                throw new BusinessRuleValidationException($"Vessel Bays ({bays.Value}) cannot exceed the vessel type max bays ({vesselType.MaxBays.Value}).");

            if (tiers.Value > vesselType.MaxTiers.Value)
                throw new BusinessRuleValidationException($"Vessel Tiers ({tiers.Value}) cannot exceed the vessel type max tiers ({vesselType.MaxTiers.Value}).");

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
            Rows = rows ;
            Bays = bays ;
            Tiers = tiers ;
            Length = length ;
        }
    }
}
