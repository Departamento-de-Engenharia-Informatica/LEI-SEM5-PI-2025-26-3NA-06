using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Location : ValueObject
    {
        public string Description { get; private set; }

        protected Location()
        {
            Description = string.Empty;
        }

        public Location(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new BusinessRuleValidationException("Location description cannot be empty.");
            Description = description.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Description;
        }
    }
}
