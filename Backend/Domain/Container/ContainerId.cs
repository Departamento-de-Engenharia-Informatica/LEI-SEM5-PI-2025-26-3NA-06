using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate
{
    public class ContainerId : EntityId
    {
        public ContainerId(Guid value) : base(value)
        {
        }
        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }
    }
}
