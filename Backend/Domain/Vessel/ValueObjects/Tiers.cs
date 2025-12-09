using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Tiers : ValueObject
    {
        public int Value { get; private set; }

        public Tiers(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Tiers must be a positive number", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
