using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public class AreaName : ValueObject
    {
        public string Value { get; private set; }

        public AreaName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleValidationException("Area name is required.");
            if (value.Length > 100)
                throw new BusinessRuleValidationException("Area name cannot exceed 100 characters.");
            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
