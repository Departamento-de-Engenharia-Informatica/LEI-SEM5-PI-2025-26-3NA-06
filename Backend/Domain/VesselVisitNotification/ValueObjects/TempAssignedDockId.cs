using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.DockAggregate;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public class TempAssignedDockId : EntityId
    {
        public TempAssignedDockId(Guid value) : base(value)
        {
        }

        public TempAssignedDockId(DockId dockId) : base(dockId.Value)
        {
        }

        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }
    }
}
