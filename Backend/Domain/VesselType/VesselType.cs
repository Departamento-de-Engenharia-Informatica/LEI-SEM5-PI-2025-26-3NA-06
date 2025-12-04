using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class VesselType : Entity<VesselTypeId>, IAggregateRoot
    {
        public TypeName TypeName { get; private set; }
        public TypeDescription TypeDescription { get; private set; }
        public TypeCapacity TypeCapacity { get; private set; }
        public MaxRows MaxRows { get; private set; }
        public MaxBays MaxBays { get; private set; }
        public MaxTiers MaxTiers { get; private set; }

        private VesselType()
        {
            TypeName = default!;
            TypeDescription = default!;
            TypeCapacity = default!;
            MaxRows = default!;
            MaxBays = default!;
            MaxTiers = default!;
        }

        public VesselType(VesselTypeId vesselTypeId, TypeName typeName, TypeDescription typeDescription,
                         TypeCapacity typeCapacity, MaxRows maxRows, MaxBays maxBays, MaxTiers maxTiers)
        {
            Id = vesselTypeId;
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            TypeDescription = typeDescription ?? throw new ArgumentNullException(nameof(typeDescription));
            TypeCapacity = typeCapacity ?? throw new ArgumentNullException(nameof(typeCapacity));
            MaxRows = maxRows ?? throw new ArgumentNullException(nameof(maxRows));
            MaxBays = maxBays ?? throw new ArgumentNullException(nameof(maxBays));
            MaxTiers = maxTiers ?? throw new ArgumentNullException(nameof(maxTiers));
        }

        public void UpdateDetails(TypeName typeName, TypeDescription typeDescription,
                                 TypeCapacity typeCapacity, MaxRows maxRows, MaxBays maxBays, MaxTiers maxTiers)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            TypeDescription = typeDescription ?? throw new ArgumentNullException(nameof(typeDescription));
            TypeCapacity = typeCapacity ?? throw new ArgumentNullException(nameof(typeCapacity));
            MaxRows = maxRows ?? throw new ArgumentNullException(nameof(maxRows));
            MaxBays = maxBays ?? throw new ArgumentNullException(nameof(maxBays));
            MaxTiers = maxTiers ?? throw new ArgumentNullException(nameof(maxTiers));
        }
    }
}
