using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class MaxRows : ValueObject
    {
        public int Value { get; private set; }

        private MaxRows()
        {
        }

        public MaxRows(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Max rows must be greater than zero", nameof(value));

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
