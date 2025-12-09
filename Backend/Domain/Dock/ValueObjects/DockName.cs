using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockName : IValueObject
    {
        public string Value { get; private set; }

        protected DockName()
        {
            Value = string.Empty;
        }

        public DockName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleValidationException("Dock name cannot be empty.");
            
            if (value.Length > 100)
                throw new BusinessRuleValidationException("Dock name cannot exceed 100 characters.");

            Value = value.Trim();
        }
    }
}
