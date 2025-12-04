using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.UserAggregate.ValueObjects
{
    public enum RoleType
    {
        Admin,
        PortAuthorityOfficer,
        LogisticOperator,
        ShippingAgentRepresentative
    }

    public class Role : ValueObject
    {
        public RoleType Value { get; private set; }

        public Role(RoleType value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }

    public static class Roles
    {
        public static Role Admin => new Role(RoleType.Admin);
        public static Role PortAuthorityOfficer => new Role(RoleType.PortAuthorityOfficer);
        public static Role LogisticOperator => new Role(RoleType.LogisticOperator);
        public static Role ShippingAgentRepresentative => new Role(RoleType.ShippingAgentRepresentative);
    }
}
