using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockId : EntityId
    {
        public DockId(Guid value) : base(value)
        {
        }

        public new Guid Value => (Guid)ObjValue;
    }
}
