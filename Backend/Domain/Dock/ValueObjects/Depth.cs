using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Depth : IValueObject
    {
        public double Value { get; private set; }

        protected Depth()
        {
            Value = 0;
        }

        public Depth(double value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Depth must be greater than 0 meters.");

            Value = value;
        }
    }
}
