using System.Text.Json.Serialization;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.UserAggregate
{
    public class UserId : EntityId
    {
        public UserId() : base(Guid.NewGuid()) { } 

        [JsonConstructor]
        public UserId(Guid value) : base(value) {}

        public UserId(string value) : base(value) {}

       

       

        public Guid AsGuid()
        {
            return (Guid)ObjValue;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null) || GetType() != obj.GetType()) return false;

            var other = (UserId)obj;
            return AsGuid().Equals(other.AsGuid());
        }

        public override int GetHashCode()
        {
            return AsGuid().GetHashCode();
        }
    }
}
