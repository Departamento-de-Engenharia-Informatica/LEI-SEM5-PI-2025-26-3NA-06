using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class MaxTiers : ValueObject
    {
        public int Value { get; private set; }

        private MaxTiers()
        {
        }

        public MaxTiers(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Max tiers must be greater than zero", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
