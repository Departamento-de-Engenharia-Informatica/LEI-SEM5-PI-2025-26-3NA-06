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
            DockName = dockName ?? throw new BusinessRuleValidationException("Dock name is required.");
            Location = location ?? throw new BusinessRuleValidationException("Location is required.");
            Length = length ?? throw new BusinessRuleValidationException("Length is required.");
            Depth = depth ?? throw new BusinessRuleValidationException("Depth is required.");
            MaxDraft = maxDraft ?? throw new BusinessRuleValidationException("Max draft is required.");
            AllowedVesselTypes = allowedVesselTypes ?? throw new BusinessRuleValidationException("Allowed vessel types are required.");
        }

        public void UpdateDetails(DockName dockName, Location location, DockLength length, Depth depth, Draft maxDraft, AllowedVesselTypes allowedVesselTypes)
        {
            DockName = dockName ?? throw new BusinessRuleValidationException("Dock name is required.");
            Location = location ?? throw new BusinessRuleValidationException("Location is required.");
            Length = length ?? throw new BusinessRuleValidationException("Length is required.");
            Depth = depth ?? throw new BusinessRuleValidationException("Depth is required.");
            MaxDraft = maxDraft ?? throw new BusinessRuleValidationException("Max draft is required.");
            AllowedVesselTypes = allowedVesselTypes ?? throw new BusinessRuleValidationException("Allowed vessel types are required.");
        }
    }
}
