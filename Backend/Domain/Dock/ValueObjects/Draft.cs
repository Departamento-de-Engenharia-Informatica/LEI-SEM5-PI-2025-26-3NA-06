using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Draft : ValueObject
    {
        public double Value { get; private set; }

        protected Draft()
        {
            Value = 0;
        }

        public Draft(double value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Max draft must be greater than 0 meters.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
