using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public enum ManifestTypeEnum
    {
        Load = 1,
        Unload = 2
    }

    public class ManifestType : ValueObject
    {
        public ManifestTypeEnum Value { get; private set; }

        public static ManifestType Load => new ManifestType(ManifestTypeEnum.Load);
        public static ManifestType Unload => new ManifestType(ManifestTypeEnum.Unload);

        protected ManifestType() { }

        public ManifestType(ManifestTypeEnum value)
        {
            if (!Enum.IsDefined(typeof(ManifestTypeEnum), value))
                throw new BusinessRuleValidationException("Invalid manifest type.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }
}
