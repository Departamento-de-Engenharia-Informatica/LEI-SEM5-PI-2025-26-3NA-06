using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockLength : IValueObject
    {
        public double Value { get; private set; }

        protected DockLength()
        {
            Value = 0;
        }

        public DockLength(double value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Dock length must be greater than 0 meters.");

            Value = value;
        }
    }
}
