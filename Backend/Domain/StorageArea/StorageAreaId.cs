using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
public class StorageAreaId : EntityId
    {
        public StorageAreaId(Guid value) : base(value) { }
        public StorageAreaId(string value) : base(Guid.Parse(value)) { }
        
        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }
    }
}