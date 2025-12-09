using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Rows : ValueObject
    {
        public int Value { get; private set; }

        public Rows(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Rows must be a positive number", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
