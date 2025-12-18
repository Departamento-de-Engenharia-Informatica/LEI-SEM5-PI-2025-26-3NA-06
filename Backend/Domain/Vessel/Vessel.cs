using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselTypeAggregate;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Vessel : Entity<VesselId>, IAggregateRoot
    {
        public IMOnumber IMO { get; private set; } = null!;
        public VesselTypeId VesselTypeId { get; private set; } = null!;
        public VesselName VesselName { get; private set; } = null!;
        public Capacity Capacity { get; private set; } = null!;
        public Rows Rows { get; private set; } = null!;
        public Bays Bays { get; private set; } = null!;
        public Tiers Tiers { get; private set; } = null!;
        public Length Length { get; private set; } = null!;

        protected Vessel()
        {}

        public Vessel(IMOnumber imo, VesselTypeId vesselTypeId, VesselName vesselName, 
                     Capacity capacity, Rows rows, Bays bays, Tiers tiers,
                     Length length)
        {
            Id = new VesselId(Guid.NewGuid());
            IMO = imo ?? throw new BusinessRuleValidationException("IMO number is required.");
            VesselTypeId = vesselTypeId ?? throw new BusinessRuleValidationException("VesselTypeId is required.");
            VesselName = vesselName ?? throw new BusinessRuleValidationException("Vessel name is required.");
            Capacity = capacity ?? throw new BusinessRuleValidationException("Capacity is required.");
            Rows = rows ?? throw new BusinessRuleValidationException("Rows are required.");
            Bays = bays ?? throw new BusinessRuleValidationException("Bays are required.");
            Tiers = tiers ?? throw new BusinessRuleValidationException("Tiers are required.");
            Length = length ?? throw new BusinessRuleValidationException("Length is required.");
        }
    
        public void UpdateDetails(VesselName vesselName, Capacity capacity, 
                                 Rows rows, Bays bays, Tiers tiers, Length length)
        {
            VesselName = vesselName ?? throw new BusinessRuleValidationException("Vessel name is required.");
            Capacity = capacity ?? throw new BusinessRuleValidationException("Capacity is required.");
            Rows = rows ?? throw new BusinessRuleValidationException("Rows are required.");
            Bays = bays ?? throw new BusinessRuleValidationException("Bays are required.");
            Tiers = tiers ?? throw new BusinessRuleValidationException("Tiers are required.");
            Length = length ?? throw new BusinessRuleValidationException("Length is required.");
        }
    }
}
