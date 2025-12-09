using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public class Dock : Entity<DockId>, IAggregateRoot
    {
        public DockName DockName { get; private set;} = null!;
        public Location Location { get; private set; } = null!;
        public DockLength Length { get; private set; } = null!;
        public Depth Depth { get; private set; } = null!;
        public Draft MaxDraft { get; private set; } = null!;
        public AllowedVesselTypes AllowedVesselTypes { get; private set; } = null!;

        protected Dock(){}

        public Dock(DockName dockName, Location location, DockLength length, Depth depth, Draft maxDraft, AllowedVesselTypes allowedVesselTypes)
        {
            Id = new DockId(Guid.NewGuid());
            DockName = dockName;
            Location = location;
            Length = length;
            Depth = depth;
            MaxDraft = maxDraft;
            AllowedVesselTypes = allowedVesselTypes;
        }

        public void ChangeDockName(DockName newDockName)
        {
            DockName = newDockName;
        }

        public void ChangeLocation(Location newLocation)
        {
            Location = newLocation;
        }

        public void ChangeLength(DockLength newLength)
        {
            Length = newLength;
        }

        public void ChangeDepth(Depth newDepth)
        {
            Depth = newDepth;
        }

        public void ChangeMaxDraft(Draft newMaxDraft)
        {
            MaxDraft = newMaxDraft;
        }

        public void SetAllowedVesselTypes(AllowedVesselTypes allowedVesselTypes)
        {
            AllowedVesselTypes = allowedVesselTypes;
        }
    }
}
