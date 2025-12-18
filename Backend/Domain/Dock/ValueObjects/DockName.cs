using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class DockName : ValueObject
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
