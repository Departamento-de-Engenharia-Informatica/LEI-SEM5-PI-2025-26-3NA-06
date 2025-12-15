using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public class VesselVisitNotificationId : EntityId
    {
        public VesselVisitNotificationId(Guid value) : base(value)
        {
        }
        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }
    }
}
