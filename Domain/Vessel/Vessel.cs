using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Vessel : Entity<IMO>, IAggregateRoot
    {
        public VesselName VesselName { get; private set; }
        public Guid OwnerId { get; private set; }
        public Guid VesselTypeId { get; private set; }
        public MaxTeu MaxTeu { get; private set; }
        public Size Size { get; private set; }
        public CargoCapacity CargoCapacity { get; private set; }

        private Vessel()
        {
            VesselName = default!;
            MaxTeu = default!;
            Size = default!;
            CargoCapacity = default!;
        }

        public Vessel(IMO imo, Guid ownerId, Guid vesselTypeId, VesselName vesselName, 
                     MaxTeu maxTeu, Size size, CargoCapacity cargoCapacity)
        {
            OwnerId = ownerId;
            VesselTypeId = vesselTypeId;
            VesselName = vesselName ?? throw new ArgumentNullException(nameof(vesselName));
            MaxTeu = maxTeu ?? throw new ArgumentNullException(nameof(maxTeu));
            Size = size ?? throw new ArgumentNullException(nameof(size));
            CargoCapacity = cargoCapacity ?? throw new ArgumentNullException(nameof(cargoCapacity));
        }
    }
}
