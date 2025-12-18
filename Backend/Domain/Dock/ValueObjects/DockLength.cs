using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockLength : ValueObject
    {
        public double Value { get; private set; }

        protected DockLength()
        {}

        public DockLength(double value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Dock length must be greater than 0 meters.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
