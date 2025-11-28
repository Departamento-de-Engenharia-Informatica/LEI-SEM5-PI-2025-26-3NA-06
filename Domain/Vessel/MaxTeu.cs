using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class MaxTeu : ValueObject
    {
        public int Value { get; private set; }

        private MaxTeu()
        {
        }

        public MaxTeu(int value)
        {
            if (value <= 0)
                throw new ArgumentException("MaxTeu must be greater than zero", nameof(value));

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
