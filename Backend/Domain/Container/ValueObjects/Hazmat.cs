using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate.ValueObjects
{
    
    public class Hazmat : ValueObject
    {
        public bool IsHazardous { get; private set; }

        public Hazmat(bool isHazardous)
        {
            IsHazardous = isHazardous;
        }

        protected Hazmat()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsHazardous;
        }

        public override string ToString()
        {
            return IsHazardous ? "Hazardous Material" : "Non-Hazardous";
        }
    }
}
