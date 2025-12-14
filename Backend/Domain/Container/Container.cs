using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate
{
    public class Container : Entity<IsoCode>, IAggregateRoot
    {
        public SizeTeu SizeTeu { get; private set; }
        public Hazmat Hazmat { get; private set; }
        public CargoType CargoType { get; private set; }
        public ContainerDescription Description { get; private set; }
        public ContainerPosition? Position { get; private set; }

        public Container(
            IsoCode isoCode,
            SizeTeu sizeTeu,
            Hazmat hazmat,
            CargoType cargoType,
            ContainerDescription description)
        {
            if (isoCode == null)
                throw new BusinessRuleValidationException("ISO Code is required.");

            if (sizeTeu == null)
                throw new BusinessRuleValidationException("Size in TEU is required.");

            if (hazmat == null)
                throw new BusinessRuleValidationException("Hazmat flag is required.");

            if (cargoType == null)
                throw new BusinessRuleValidationException("Cargo type is required.");

            if (description == null)
                throw new BusinessRuleValidationException("Description is required.");

            Id = isoCode;
            SizeTeu = sizeTeu;
            Hazmat = hazmat;
            CargoType = cargoType;
            Description = description;
        }

        protected Container()
        {
            Id = default!;
            SizeTeu = default!;
            Hazmat = default!;
            CargoType = default!;
            Description = default!;
        }

        public void UpdateCargoInformation(CargoType cargoType, ContainerDescription description)
        {
            if (cargoType == null)
                throw new BusinessRuleValidationException("Cargo type is required.");

            if (description == null)
                throw new BusinessRuleValidationException("Description is required.");

            CargoType = cargoType;
            Description = description;
        }

        public void SetPosition(ContainerPosition position)
        {
            if (position == null)
                throw new BusinessRuleValidationException("Position is required.");

            Position = position;
        }

        public void ClearPosition()
        {
            Position = null;
        }

        public override string ToString()
        {
            return $"Container {Id} - {CargoType} ({SizeTeu})";
        }
    }
}
