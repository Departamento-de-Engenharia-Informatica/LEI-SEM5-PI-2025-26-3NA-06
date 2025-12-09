using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class AllowedVesselTypes : IValueObject
    {
        public List<Guid> VesselTypeIds { get; private set; }

        public AllowedVesselTypes(List<Guid>? vesselTypeIds)
        {
            VesselTypeIds = vesselTypeIds ?? new List<Guid>();
        }

        public AllowedVesselTypes() : this(null) { }
    }
}
