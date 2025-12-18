using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class AllowedVesselTypes : ValueObject
    {
        public List<Guid> VesselTypeIds { get; private set; }

        public AllowedVesselTypes(List<Guid>? vesselTypeIds)
        {
            VesselTypeIds = vesselTypeIds ?? new List<Guid>();
        }

        public AllowedVesselTypes() : this(null) { }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return VesselTypeIds;
        }
    }
}
