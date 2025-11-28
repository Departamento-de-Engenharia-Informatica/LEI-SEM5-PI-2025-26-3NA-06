using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class VesselName : ValueObject
    {
        public string Name { get; private set; }

        private VesselName()
        {
        }

        public VesselName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Vessel name cannot be empty", nameof(name));

            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
