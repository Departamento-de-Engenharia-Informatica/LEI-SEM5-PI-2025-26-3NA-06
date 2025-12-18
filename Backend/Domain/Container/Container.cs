using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate
{
    public class Container : Entity<ContainerId>, IAggregateRoot
    {
        public IsoCode IsoCode { get; private set; } = null!;
        public bool IsHazardous { get; private set; } = false;
        public CargoType CargoType { get; private set; } = null!;
        public ContainerDescription Description { get; private set; } = null!;

        protected Container()
        {
        }

        public Container(
            IsoCode isoCode,
            bool isHazardous,
            CargoType cargoType,
            ContainerDescription description)
        {
            Id = new ContainerId(Guid.NewGuid());
            IsoCode = isoCode ?? throw new BusinessRuleValidationException("ISO Code is required."); 
            IsHazardous = isHazardous; 
            CargoType = cargoType ?? throw new BusinessRuleValidationException("Cargo type is required.");
            Description = description ?? throw new BusinessRuleValidationException("Description is required.");
        }

        public void UpdateCargoInformation(CargoType cargoType, ContainerDescription description, bool isHazardous)
        {
            IsHazardous = isHazardous;
            CargoType = cargoType ?? throw new BusinessRuleValidationException("Cargo type is required.");
            Description = description ?? throw new BusinessRuleValidationException("Description is required.");
        }

        public override string ToString()
        {
            return $"Container {Id} - {CargoType} - {Description} - Hazmat: {IsHazardous}";
        }
    }
}
