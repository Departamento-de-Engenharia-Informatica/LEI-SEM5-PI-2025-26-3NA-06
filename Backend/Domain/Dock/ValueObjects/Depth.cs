using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Depth : ValueObject
    {
        public double Value { get; private set; }

        protected Depth()
        {}

        public Depth(double value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Depth must be greater than 0 meters.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
