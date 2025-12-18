using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class VesselType : Entity<VesselTypeId>, IAggregateRoot
    {
        public TypeName TypeName { get; private set; } = null!;
        public TypeDescription TypeDescription { get; private set; } = null!;
        public TypeCapacity TypeCapacity { get; private set; } = null!;
        public MaxRows MaxRows { get; private set; } = null!;
        public MaxBays MaxBays { get; private set; } = null!;
        public MaxTiers MaxTiers { get; private set; } = null!;

        protected VesselType() {}
        public VesselType(TypeName typeName, TypeDescription typeDescription,
                         TypeCapacity typeCapacity, MaxRows maxRows, MaxBays maxBays, MaxTiers maxTiers)
        {
            Id = new VesselTypeId(Guid.NewGuid()); 
            TypeName = typeName ?? throw new BusinessRuleValidationException("Type name is required.");
            TypeDescription = typeDescription ?? throw new BusinessRuleValidationException("Type description is required.");
            TypeCapacity = typeCapacity ?? throw new BusinessRuleValidationException("Type capacity is required.");
            MaxRows = maxRows ?? throw new BusinessRuleValidationException("Max rows is required.");
            MaxBays = maxBays ?? throw new BusinessRuleValidationException("Max bays is required.");
            MaxTiers = maxTiers ?? throw new BusinessRuleValidationException("Max tiers is required.");
        }

        public void UpdateDetails(TypeName typeName, TypeDescription typeDescription,
                                 TypeCapacity typeCapacity, MaxRows maxRows, MaxBays maxBays, MaxTiers maxTiers)
        {
            TypeName = typeName ?? throw new BusinessRuleValidationException("Type name is required.");
            TypeDescription = typeDescription ?? throw new BusinessRuleValidationException("Type description is required.");
            TypeCapacity = typeCapacity ?? throw new BusinessRuleValidationException("Type capacity is required.");
            MaxRows = maxRows ?? throw new BusinessRuleValidationException("Max rows is required.");
            MaxBays = maxBays ?? throw new BusinessRuleValidationException("Max bays is required.");
            MaxTiers = maxTiers ?? throw new BusinessRuleValidationException("Max tiers is required.");
        }
    }
}
