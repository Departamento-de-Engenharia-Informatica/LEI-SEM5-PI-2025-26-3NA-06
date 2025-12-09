using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Length : ValueObject
    {
        public double Value { get; private set; }

        private Length()
        {
        }

        public Length(double value)
        {
            if (value <= 0)
                throw new ArgumentException("Length must be greater than zero", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return $"{Value} meters";
        }
    }
}
