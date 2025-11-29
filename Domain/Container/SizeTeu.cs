using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.Container
{
    /// <summary>
    /// Container size in TEU (Twenty-foot Equivalent Units)
    /// Typical values: 1 TEU (20-foot container), 2 TEU (40-foot container)
    /// </summary>
    public class SizeTeu : ValueObject
    {
        public int Value { get; private set; }

        public SizeTeu(int value)
        {
            if (value <= 0)
                throw new BusinessRuleValidationException("Container size must be greater than zero.");

            if (value > 4)
                throw new BusinessRuleValidationException(
                    "Container size cannot exceed 4 TEU (standard maximum is 2 TEU for 40-foot containers).");

            Value = value;
        }

        protected SizeTeu()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return $"{Value} TEU";
        }
    }
}
