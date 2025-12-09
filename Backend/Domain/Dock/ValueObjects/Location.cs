using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Location : IValueObject
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
    }
}
