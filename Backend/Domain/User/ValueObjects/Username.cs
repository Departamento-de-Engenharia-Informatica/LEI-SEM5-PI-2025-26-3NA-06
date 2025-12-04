using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.UserAggregate.ValueObjects
{
    public class Username : ValueObject
    {
        public string Value { get; private set; }

        public Username(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleValidationException("Username cannot be empty.");

            if (value.Length < 3)
                throw new BusinessRuleValidationException("Username must be at least 3 characters.");

            if (value.Length > 50)
                throw new BusinessRuleValidationException("Username must be at most 20 characters.");

            if (char.IsDigit(value[0]))
                throw new BusinessRuleValidationException("Username cannot start with a number.");

            if (value.Contains(" "))
                throw new BusinessRuleValidationException("Username cannot contain spaces.");

            if (value.EndsWith(".") || value.EndsWith("_"))
                throw new BusinessRuleValidationException("Username cannot end with a dot or underscore.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
