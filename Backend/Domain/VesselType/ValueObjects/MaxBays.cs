using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class MaxBays : ValueObject
    {
        public int Value { get; private set; }

        private MaxBays()
        {
        }

        public MaxBays(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Max bays must be greater than zero", nameof(value));

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
