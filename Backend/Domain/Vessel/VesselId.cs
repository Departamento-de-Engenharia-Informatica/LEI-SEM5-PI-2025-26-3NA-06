using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class VesselId : EntityId
    {
        private VesselId() : base(Guid.Empty)
        {
        }

        public VesselId(Guid id) : base(id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("VesselId cannot be empty", nameof(id));
        }

        public new Guid Value => (Guid)ObjValue;
    }
}
