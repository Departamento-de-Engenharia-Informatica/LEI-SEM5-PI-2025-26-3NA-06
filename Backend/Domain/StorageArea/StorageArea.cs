using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public class StorageArea : Entity<StorageAreaId>, IAggregateRoot
    {
        public AreaName Name { get; private set; } = null!;
        public AreaType AreaType { get; private set; } = null!;
        public Location Location { get; private set; } = null!;
        public MaxCapacity MaxCapacity { get; private set; } = null!;
        public CurrentOccupancy CurrentOccupancy { get; } = null!;
        public bool ServesEntirePort { get; private set; } = true;
        public List<IsoCode> CurrentContainers { get; private set; } = null!;
        public List<DockId> ServedDocks { get; private set; } = null!;

        protected StorageArea() { }
        public StorageArea( AreaName name, AreaType type, Location location, MaxCapacity maxCapacity,bool servesEntirePort, List<DockId> servedDocks)
        {
            Id = new StorageAreaId(Guid.NewGuid());
            Name = name;
            AreaType = type;
            Location = location;
            MaxCapacity = maxCapacity;
            ServesEntirePort = servesEntirePort;
            ServedDocks = servedDocks;
        }

       public void UpdateDetails(AreaName name, AreaType type, Location location, MaxCapacity maxCapacity,bool servesEntirePort, List<DockId> servedDocks)
        {
            Name = name;
            AreaType = type;
            Location = location;
            MaxCapacity = maxCapacity;
            ServesEntirePort = servesEntirePort;
            ServedDocks = servedDocks;
        }
    }

    
}
//TER EM ATENÇÃO AQUI: Updates to storage areas must not allow the current occupancy to exceed maximum capacity.
//Quando adicionarmos Containers a uma Storage Area, temos de garantir que o CurrentOccupancy não ultrapassa o MaxCapacity.