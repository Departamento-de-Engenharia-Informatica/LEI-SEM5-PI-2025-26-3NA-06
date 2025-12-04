using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Size : ValueObject
    {
        public double Value { get; private set; }

        private Size()
        {
        }

        public Size(double value)
        {
            if (value <= 0)
                throw new ArgumentException("Size must be greater than zero", nameof(value));

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
