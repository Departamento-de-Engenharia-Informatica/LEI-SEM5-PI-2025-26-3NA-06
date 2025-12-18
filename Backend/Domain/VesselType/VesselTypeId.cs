using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class VesselTypeId : EntityId
    {
        private VesselTypeId() : base(Guid.Empty)
        {
        }

        public VesselTypeId(Guid id) : base(id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("VesselTypeId cannot be empty", nameof(id));
        }

        public new Guid Value => (Guid)ObjValue;
    }
}
