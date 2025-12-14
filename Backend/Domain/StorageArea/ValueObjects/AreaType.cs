using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public enum AreaTypeEnum
    {
        Yard = 0,
        Warehouse = 1
    }

    public class AreaType : ValueObject
    {
        public AreaTypeEnum Value { get; private set; }

        public AreaType(AreaTypeEnum value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }

    public static class AreaTypes
    {
        public static AreaType Yard => new AreaType(AreaTypeEnum.Yard);
        public static AreaType Warehouse => new AreaType(AreaTypeEnum.Warehouse);
    }
}