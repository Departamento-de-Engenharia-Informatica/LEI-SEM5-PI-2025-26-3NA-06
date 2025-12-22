using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.DockAggregate;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    // This entity can be saved partially. Only the ReferedVesselId and Status is mandatory at creation.
    public class VesselVisitNotification : Entity<VesselVisitNotificationId>, IAggregateRoot
    {
        public ReferredVesselId ReferredVesselId { get; private set; } = null!;
        public ArrivalDate? ArrivalDate { get; private set; }
        public DepartureDate? DepartureDate { get; private set; }

        // Cargo manifests: 0, 1, or 2 manifests (loading and/or unloading)
        private readonly List<CargoManifest> _cargoManifests = [];
        public IReadOnlyCollection<CargoManifest> CargoManifests => _cargoManifests.AsReadOnly();
        public bool IsHazardous { get; private set; } = false;
        //public CrewMembersList CrewMembersList { get; private set; } = null!;
        public RejectionReason? RejectionReason { get; private set; }
        public TempAssignedDockId? TempAssignedDockId { get; private set; }
        public Status Status { get; private set; } = null!;

        // Primitive property for EF Core queries
        public int StatusValue
        {
            get => (int)Status.Value;
            private set => Status = new Status((StatusEnum)value);
        }

    protected VesselVisitNotification() { }

    public VesselVisitNotification(string referredVessel, DateTime? arrivalDate, DateTime? departureDate)
    {
        Id = new VesselVisitNotificationId(Guid.NewGuid());
        ReferredVesselId = new ReferredVesselId(new IMOnumber(referredVessel));
        ArrivalDate = arrivalDate.HasValue ? new ArrivalDate(arrivalDate) : null;
        DepartureDate = departureDate.HasValue ? new DepartureDate(departureDate) : null;
        Status = Statuses.InProgress;
    }

    // Update dates (for draft editing)
    public void UpdateDates(DateTime? arrivalDate, DateTime? departureDate)
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Dates can only be updated for notifications in progress.");
        
        ArrivalDate = arrivalDate.HasValue ? new ArrivalDate(arrivalDate) : null;
        DepartureDate = departureDate.HasValue ? new DepartureDate(departureDate) : null;
    }

    // Manifest management methods
    public void SetLoadingManifest(CargoManifest manifest)
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Manifests can only be modified for notifications in progress.");
        
        if (manifest.ManifestType.Value != ManifestTypeEnum.Load)
            throw new BusinessRuleValidationException("Manifest must be of type Load.");

        // Remove existing loading manifest if any
        _cargoManifests.RemoveAll(m => m.ManifestType.Value == ManifestTypeEnum.Load);
        _cargoManifests.Add(manifest);
    }

    public void SetUnloadingManifest(CargoManifest manifest)
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Manifests can only be modified for notifications in progress.");
        
        if (manifest.ManifestType.Value != ManifestTypeEnum.Unload)
            throw new BusinessRuleValidationException("Manifest must be of type Unload.");

        // Remove existing unloading manifest if any
        _cargoManifests.RemoveAll(m => m.ManifestType.Value == ManifestTypeEnum.Unload);
        _cargoManifests.Add(manifest);
    }

    public void RemoveLoadingManifest()
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Manifests can only be modified for notifications in progress.");
        
        _cargoManifests.RemoveAll(m => m.ManifestType.Value == ManifestTypeEnum.Load);
    }

    public void RemoveUnloadingManifest()
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Manifests can only be modified for notifications in progress.");
        
        _cargoManifests.RemoveAll(m => m.ManifestType.Value == ManifestTypeEnum.Unload);
    }

    public CargoManifest? GetLoadingManifest()
    {
        return _cargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Load);
    }

    public CargoManifest? GetUnloadingManifest()
    {
        return _cargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Unload);
    }
    
    public void Submit()
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new InvalidOperationException("Only notifications in progress can be submitted.");

        // Full validation for submission
        ValidateForSubmission();

        Status = Statuses.Submitted;
    }

    private void ValidateForSubmission()
    {
        // 1. Dates must be present and valid
        if (ArrivalDate == null)
            throw new BusinessRuleValidationException("Arrival date is required for submission.");
        
        if (DepartureDate == null)
            throw new BusinessRuleValidationException("Departure date is required for submission.");

        if (ArrivalDate.Value >= DepartureDate.Value)
            throw new BusinessRuleValidationException("Arrival date must be before departure date.");

        // 2. Validate manifest consistency if manifests exist
        foreach (var manifest in _cargoManifests)
        {
            manifest.ValidateConsistency();
        }

        // 3. Check for duplicate containers across different manifests
        var loadManifest = GetLoadingManifest();
        var unloadManifest = GetUnloadingManifest();

        if (loadManifest != null && unloadManifest != null)
        {
            var loadContainerIds = loadManifest.Entries.Select(e => e.ContainerId).ToList();
            var unloadContainerIds = unloadManifest.Entries.Select(e => e.ContainerId).ToList();
            
            var duplicates = loadContainerIds.Intersect(unloadContainerIds).ToList();
            
            if (duplicates.Any())
                throw new BusinessRuleValidationException(
                    $"Containers cannot appear in both loading and unloading manifests: {string.Join(", ", duplicates)}");
        }

        // Note: Referenced Container IDs and StorageArea IDs existence validation
        // will be done in the service layer with repository checks
    }

    public void Accept()
    {
        if (!Status.Equals(Statuses.Submitted))
            throw new InvalidOperationException("Only submitted notifications can be accepted.");
        Status = Statuses.Accepted;
        RejectionReason = null;
    }

    public void Approve(DockId tempAssignedDockId, string officerId)
    {
        if (!Status.Equals(Statuses.Submitted))
            throw new BusinessRuleValidationException("Only submitted notifications can be approved.");
        
        if (tempAssignedDockId == null)
            throw new BusinessRuleValidationException("Temporary assigned dock is required for approval.");
        
        if (string.IsNullOrWhiteSpace(officerId))
            throw new BusinessRuleValidationException("Officer ID is required.");

        Status = Statuses.Accepted;
        TempAssignedDockId = new TempAssignedDockId(tempAssignedDockId);
        RejectionReason = null;
    
    }

    public void Reject(string rejectionReason, string officerId)
    {
        if (!Status.Equals(Statuses.Submitted))
            throw new BusinessRuleValidationException("Only submitted notifications can be rejected.");
        
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new BusinessRuleValidationException("Rejection reason is required.");
        
        if (string.IsNullOrWhiteSpace(officerId))
            throw new BusinessRuleValidationException("Officer ID is required.");

        Status = Statuses.Rejected;
        RejectionReason = new RejectionReason(rejectionReason);
        TempAssignedDockId = null;
    }

    public void Resubmit()
    {
        if (!Status.Equals(Statuses.Rejected) && !Status.Equals(Statuses.InProgress))
            throw new BusinessRuleValidationException("Only rejected or in-progress notifications can be resubmitted.");

        // Full validation for submission
        ValidateForSubmission();

        Status = Statuses.Submitted;
        RejectionReason = null;
    }

    // Update and resubmit a rejected VVN with new data
    public void UpdateAndResubmit(DateTime? arrivalDate, DateTime? departureDate)
    {
        if (!Status.Equals(Statuses.Rejected))
            throw new BusinessRuleValidationException("Only rejected notifications can be updated and resubmitted.");

        // Temporarily set to InProgress to allow updates
        Status = Statuses.InProgress;
        
        // Update dates
        ArrivalDate = arrivalDate.HasValue ? new ArrivalDate(arrivalDate) : null;
        DepartureDate = departureDate.HasValue ? new DepartureDate(departureDate) : null;
    }

    public void UpdateManifestsForResubmit(CargoManifest? loadingManifest, CargoManifest? unloadingManifest)
    {
        if (!Status.Equals(Statuses.InProgress))
            throw new BusinessRuleValidationException("Manifests can only be updated when status is InProgress.");

        // Clear existing manifests
        _cargoManifests.Clear();

        // Add new manifests
        if (loadingManifest != null)
        {
            if (loadingManifest.ManifestType.Value != ManifestTypeEnum.Load)
                throw new BusinessRuleValidationException("Loading manifest must be of type Load.");
            _cargoManifests.Add(loadingManifest);
        }

        if (unloadingManifest != null)
        {
            if (unloadingManifest.ManifestType.Value != ManifestTypeEnum.Unload)
                throw new BusinessRuleValidationException("Unloading manifest must be of type Unload.");
            _cargoManifests.Add(unloadingManifest);
        }
    }

    // Convert rejected VVN back to draft status
    public void ConvertToDraft()
    {
        if (!Status.Equals(Statuses.Rejected))
            throw new BusinessRuleValidationException("Only rejected notifications can be converted back to draft.");

        Status = Statuses.InProgress;
        RejectionReason = null;
    }

    // Update IsHazardous based on containers in manifests
    public void UpdateHazardousStatus(IEnumerable<Container> allContainers)
    {
        // Get all container IDs from manifests
        var containerIds = _cargoManifests
            .SelectMany(m => m.Entries)
            .Select(e => e.ContainerId)
            .Distinct()
            .ToList();

        // Check if any of these containers are hazardous
        IsHazardous = allContainers
            .Where(c => containerIds.Contains(c.Id))
            .Any(c => c.IsHazardous);
    }
    }
}
