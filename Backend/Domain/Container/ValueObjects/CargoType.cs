using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate.ValueObjects
{
    public class CargoType : ValueObject
    {
        public string Type { get; private set; }

        public CargoType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new BusinessRuleValidationException("Cargo type cannot be empty.");

            if (type.Length < 3)
                throw new BusinessRuleValidationException("Cargo type must be at least 3 characters long.");

            if (type.Length > 50)
                throw new BusinessRuleValidationException("Cargo type cannot exceed 50 characters.");

            Type = type.Trim();
        }

        protected CargoType()
        {
            Type = default!;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}
