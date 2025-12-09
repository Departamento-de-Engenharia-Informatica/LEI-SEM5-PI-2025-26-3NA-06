using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Bays : ValueObject
    {
        public int Value { get; private set; }

        public Bays(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Bays must be a positive number", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
