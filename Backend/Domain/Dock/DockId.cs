using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockId : EntityId
    {
        public DockId(Guid value) : base(value)
        {
        }
        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }
    }
}
