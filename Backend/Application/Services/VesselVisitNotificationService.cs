using ProjArqsi.Domain.Shared;
using AutoMapper;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Application.DTOs.VVN;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Application.Logging;

namespace ProjArqsi.Application.Services
{
    public class VesselVisitNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVesselVisitNotificationRepository _repo;
        private readonly IContainerRepository _containerRepo;
        private readonly IStorageAreaRepository _storageAreaRepo;
        private readonly IVesselRepository _vesselRepo;
        private readonly IDockRepository _dockRepo;
        private readonly IMapper _mapper;
        private readonly ICoreApiLogger _apiLogger;

        public VesselVisitNotificationService(
            IUnitOfWork unitOfWork, 
            IVesselVisitNotificationRepository repo,
            IContainerRepository containerRepo,
            IStorageAreaRepository storageAreaRepo,
            IVesselRepository vesselRepo,
            IDockRepository dockRepo,
            IMapper mapper,
            ICoreApiLogger apiLogger)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _containerRepo = containerRepo;
            _storageAreaRepo = storageAreaRepo;
            _vesselRepo = vesselRepo;
            _dockRepo = dockRepo;
            _mapper = mapper;
            _apiLogger = apiLogger;
        }

        //This stores draft VVN. Only basic checks are performed. The object will suffer changes later.
        public async Task<VVNDraftDtoWId> DraftVVNAsync(VVNDraftDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReferredVesselId))
                throw new ArgumentException("Vessel ID (IMO number) is required.");

            var draft = new VesselVisitNotification(dto.ReferredVesselId, dto.ArrivalDate, dto.DepartureDate);

            // Process manifests if provided (optional for draft)
            if (dto.LoadingManifest != null)
            {
                var loadManifest = await BuildCargoManifestAsync(dto.LoadingManifest, ManifestTypeEnum.Load, validateReferences: false);
                draft.SetLoadingManifest(loadManifest);
            }

            if (dto.UnloadingManifest != null)
            {
                var unloadManifest = await BuildCargoManifestAsync(dto.UnloadingManifest, ManifestTypeEnum.Unload, validateReferences: false);
                draft.SetUnloadingManifest(unloadManifest);
            }

            await _repo.DraftVVN(draft);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNDraftDtoWId>(draft);
        }

        // Update an existing draft
        public async Task<VVNDraftDtoWId> UpdateDraftAsync(Guid id, VVNDraftDto dto)
        {
            var draft = await _repo.GetDraftByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Draft not found.");

            // Update dates
            draft.UpdateDates(dto.ArrivalDate, dto.DepartureDate);

            // Update manifests
            if (dto.LoadingManifest != null)
            {
                var loadManifest = await BuildCargoManifestAsync(dto.LoadingManifest, ManifestTypeEnum.Load, validateReferences: false);
                draft.SetLoadingManifest(loadManifest);
            }
            else
            {
                draft.RemoveLoadingManifest();
            }

            if (dto.UnloadingManifest != null)
            {
                var unloadManifest = await BuildCargoManifestAsync(dto.UnloadingManifest, ManifestTypeEnum.Unload, validateReferences: false);
                draft.SetUnloadingManifest(unloadManifest);
            }
            else
            {
                draft.RemoveUnloadingManifest();
            }

            // Update hazardous status based on containers
            var allContainers = await _containerRepo.GetAllAsync();
            draft.UpdateHazardousStatus(allContainers);

            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNDraftDtoWId>(draft);
        }

        //This stores finished VVN. All business rules must be validated.
        public async Task<VVNSubmitDtoWId> SubmitVVNAsync(VVNSubmitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReferredVesselId))
                throw new ArgumentException("Vessel ID (IMO number) is required.");
            if (dto.ArrivalDate == default)
                throw new ArgumentException("Arrival date is required.");
            if (dto.DepartureDate == default)
                throw new ArgumentException("Departure date is required.");
            if (dto.DepartureDate <= dto.ArrivalDate)
                throw new ArgumentException("Departure date must be after arrival date.");
           
            // Validate vessel exists
            var vessel = await _vesselRepo.GetByImoAsync(new IMOnumber(dto.ReferredVesselId));
            if (vessel == null)
                throw new BusinessRuleValidationException($"Vessel with IMO '{dto.ReferredVesselId}' not found.");

            // Check for conflicting VVNs
            var conflicts = await _repo.GetConflictingVvnsForVesselAsync(
                new IMOnumber(dto.ReferredVesselId), 
                dto.ArrivalDate, 
                dto.DepartureDate);
            
            if (conflicts.Any())
            {
                var conflictDetails = conflicts.Select(c => 
                    $"VVN {c.Id.AsGuid()} from {c.ArrivalDate?.Value:yyyy-MM-dd HH:mm} to {c.DepartureDate?.Value:yyyy-MM-dd HH:mm}");
                throw new BusinessRuleValidationException(
                    $"Vessel with IMO '{dto.ReferredVesselId}' already has conflicting visit notification(s) during this time period: {string.Join(", ", conflictDetails)}");
            }

            var vvn = new VesselVisitNotification(dto.ReferredVesselId, dto.ArrivalDate, dto.DepartureDate);

            // Process manifests with full validation
            if (dto.LoadingManifest != null)
            {
                var loadManifest = await BuildCargoManifestAsync(dto.LoadingManifest, ManifestTypeEnum.Load, validateReferences: true);
                vvn.SetLoadingManifest(loadManifest);
            }

            if (dto.UnloadingManifest != null)
            {
                var unloadManifest = await BuildCargoManifestAsync(dto.UnloadingManifest, ManifestTypeEnum.Unload, validateReferences: true);
                vvn.SetUnloadingManifest(unloadManifest);
            }

            // Update hazardous status based on containers
            var allContainers = await _containerRepo.GetAllAsync();
            vvn.UpdateHazardousStatus(allContainers);

            // Submit (performs domain validation)
            vvn.Submit();
            
            await _repo.AddAsync(vvn);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNSubmitDtoWId>(vvn);
        }

        // Submit an existing draft
        public async Task<VVNSubmitDtoWId> SubmitDraftAsync(Guid id)
        {
            var draft = await _repo.GetDraftByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Draft not found.");

            // Validate vessel exists
            var vesselIMO = draft.ReferredVesselId.VesselId.Value;
            var vessel = await _vesselRepo.GetByImoAsync(new IMOnumber(vesselIMO));
            if (vessel == null)
                throw new BusinessRuleValidationException($"Vessel with IMO '{vesselIMO}' not found.");

            // Check for conflicting VVNs
            if (draft.ArrivalDate != null && draft.DepartureDate != null)
            {
                var conflicts = await _repo.GetConflictingVvnsForVesselAsync(
                    new IMOnumber(vesselIMO),
                    draft.ArrivalDate.Value!.Value,
                    draft.DepartureDate.Value!.Value,
                    draft.Id);
                
                if (conflicts.Any())
                {
                    var conflictDetails = conflicts.Select(c => 
                        $"VVN {c.Id.AsGuid()} from {c.ArrivalDate?.Value:yyyy-MM-dd HH:mm} to {c.DepartureDate?.Value:yyyy-MM-dd HH:mm}");
                    throw new BusinessRuleValidationException(
                        $"Vessel with IMO '{vesselIMO}' already has conflicting visit notification(s) during this time period: {string.Join(", ", conflictDetails)}");
                }
            }

            // Validate all referenced entities exist
            await ValidateManifestReferencesAsync(draft);

            // Update hazardous status based on containers
            var allContainers = await _containerRepo.GetAllAsync();
            draft.UpdateHazardousStatus(allContainers);

            // Submit (performs domain validation including date checks)
            draft.Submit();
            
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNSubmitDtoWId>(draft);
        }

        private async Task<CargoManifest> BuildCargoManifestAsync(CargoManifestDto dto, ManifestTypeEnum expectedType, bool validateReferences)
        {
            if (!Enum.TryParse<ManifestTypeEnum>(dto.ManifestType, true, out var manifestType))
                throw new ArgumentException($"Invalid manifest type: {dto.ManifestType}");

            if (manifestType != expectedType)
                throw new ArgumentException($"Expected manifest type {expectedType}, but got {manifestType}");

            var manifest = new CargoManifest(new ManifestType(manifestType));

            foreach (var entryDto in dto.Entries)
            {
                if (!Guid.TryParse(entryDto.ContainerId, out var containerGuid))
                    throw new ArgumentException($"Invalid container ID: {entryDto.ContainerId}");

                var containerId = new ContainerId(containerGuid);

                // Validate container exists if required
                if (validateReferences)
                {
                    var container = await _containerRepo.GetByIdAsync(containerId);
                    if (container == null)
                        throw new BusinessRuleValidationException($"Container with ID '{containerGuid}' not found.");
                }

                ManifestEntry entry;

                if (manifestType == ManifestTypeEnum.Load)
                {
                    if (string.IsNullOrWhiteSpace(entryDto.SourceStorageAreaId))
                        throw new ArgumentException("Source storage area is required for loading operations.");

                    if (!Guid.TryParse(entryDto.SourceStorageAreaId, out var sourceGuid))
                        throw new ArgumentException($"Invalid source storage area ID: {entryDto.SourceStorageAreaId}");

                    var sourceStorageAreaId = new StorageAreaId(sourceGuid);

                    // Validate storage area exists if required
                    if (validateReferences)
                    {
                        var storageArea = await _storageAreaRepo.GetByIdAsync(sourceStorageAreaId);
                        if (storageArea == null)
                            throw new BusinessRuleValidationException($"Storage area with ID '{sourceGuid}' not found.");
                    }

                    entry = ManifestEntry.CreateLoadEntry(containerId, sourceStorageAreaId);
                }
                else // Unload
                {
                    if (string.IsNullOrWhiteSpace(entryDto.TargetStorageAreaId))
                        throw new ArgumentException("Target storage area is required for unloading operations.");

                    if (!Guid.TryParse(entryDto.TargetStorageAreaId, out var targetGuid))
                        throw new ArgumentException($"Invalid target storage area ID: {entryDto.TargetStorageAreaId}");

                    var targetStorageAreaId = new StorageAreaId(targetGuid);

                    // Validate storage area exists if required
                    if (validateReferences)
                    {
                        var storageArea = await _storageAreaRepo.GetByIdAsync(targetStorageAreaId);
                        if (storageArea == null)
                            throw new BusinessRuleValidationException($"Storage area with ID '{targetGuid}' not found.");
                    }

                    entry = ManifestEntry.CreateUnloadEntry(containerId, targetStorageAreaId);
                }

                manifest.AddEntry(entry);
            }

            return manifest;
        }

        private async Task ValidateManifestReferencesAsync(VesselVisitNotification vvn)
        {
            var allContainerIds = new HashSet<ContainerId>();

            foreach (var manifest in vvn.CargoManifests)
            {
                foreach (var entry in manifest.Entries)
                {
                    // Validate container exists
                    var container = await _containerRepo.GetByIdAsync(entry.ContainerId);
                    if (container == null)
                        throw new BusinessRuleValidationException($"Container with ID '{entry.ContainerId.AsGuid()}' not found.");

                    allContainerIds.Add(entry.ContainerId);

                    // Validate source storage area if present
                    if (entry.SourceStorageAreaId != null)
                    {
                        var sourceArea = await _storageAreaRepo.GetByIdAsync(entry.SourceStorageAreaId);
                        if (sourceArea == null)
                            throw new BusinessRuleValidationException($"Source storage area with ID '{entry.SourceStorageAreaId.AsGuid()}' not found.");
                    }

                    // Validate target storage area if present
                    if (entry.TargetStorageAreaId != null)
                    {
                        var targetArea = await _storageAreaRepo.GetByIdAsync(entry.TargetStorageAreaId);
                        if (targetArea == null)
                            throw new BusinessRuleValidationException($"Target storage area with ID '{entry.TargetStorageAreaId.AsGuid()}' not found.");
                    }
                }
            }
        }

        public async Task<VVNDto> ApproveVvnAsync(Guid id, Guid tempAssignedDockId, string officerId, bool confirmDockConflict = false)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            // Validate dock exists
            var dock = await _dockRepo.GetByIdAsync(new DockId(tempAssignedDockId))
                ?? throw new BusinessRuleValidationException($"Dock with ID '{tempAssignedDockId}' not found.");

            // Get vessel to check its type
            var vessel = await _vesselRepo.GetByImoAsync(new IMOnumber(vvn.ReferredVesselId.VesselId.Value))
                ?? throw new BusinessRuleValidationException($"Vessel with IMO '{vvn.ReferredVesselId.VesselId.Value}' not found.");

            // Validate that the dock can accommodate this vessel type
            if (!dock.AllowedVesselTypes.VesselTypeIds.Contains(vessel.VesselTypeId.AsGuid()))
            {
                throw new BusinessRuleValidationException($"The selected dock '{dock.DockName.Value}' cannot accommodate vessels of this type. Please select a different dock.");
            }

            // Check for dock conflicts if dates are present
            if (vvn.ArrivalDate != null && vvn.DepartureDate != null)
            {
                var dockConflicts = await _repo.GetConflictingVvnsForDockAsync(
                    tempAssignedDockId,
                    vvn.ArrivalDate.Value!.Value,
                    vvn.DepartureDate.Value!.Value,
                    vvn.Id);

                if (dockConflicts.Any() && !confirmDockConflict)
                {
                    var conflictDetails = dockConflicts.Select(c =>
                        $"VVN {c.Id.AsGuid()} (Vessel IMO: {c.ReferredVesselId.VesselId.Value}) from {c.ArrivalDate?.Value:yyyy-MM-dd HH:mm} to {c.DepartureDate?.Value:yyyy-MM-dd HH:mm}");
                    
                    throw new BusinessRuleValidationException(
                        $"DOCK_CONFLICT: Dock '{dock.DockName.Value}' is already assigned to other vessel(s) during this time period: {string.Join("; ", conflictDetails)}. Set ConfirmDockConflict=true to approve anyway.");
                }
            }

            vvn.Approve(new DockId(tempAssignedDockId), officerId);
            await _unitOfWork.CommitAsync();

            _apiLogger.LogVvnApproved(id, tempAssignedDockId, officerId);

            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<DockConflictInfoDto> CheckDockConflictsAsync(Guid vvnId, Guid dockId)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(vvnId))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            if (vvn.ArrivalDate == null || vvn.DepartureDate == null)
            {
                return new DockConflictInfoDto { HasConflicts = false };
            }

            var conflicts = await _repo.GetConflictingVvnsForDockAsync(
                dockId,
                vvn.ArrivalDate.Value!.Value,
                vvn.DepartureDate.Value!.Value,
                vvn.Id);

            if (!conflicts.Any())
            {
                return new DockConflictInfoDto { HasConflicts = false };
            }

            var conflictingVvns = new List<ConflictingVvnDto>();
            foreach (var conflict in conflicts)
            {
                var vessel = await _vesselRepo.GetByImoAsync(new IMOnumber(conflict.ReferredVesselId.VesselId.Value));
                
                conflictingVvns.Add(new ConflictingVvnDto
                {
                    VvnId = conflict.Id.AsGuid().ToString(),
                    VesselImo = conflict.ReferredVesselId.VesselId.Value,
                    VesselName = vessel?.VesselName.Name ?? "Unknown",
                    ArrivalDate = conflict.ArrivalDate!.Value!.Value,
                    DepartureDate = conflict.DepartureDate!.Value!.Value
                });
            }

            return new DockConflictInfoDto
            {
                HasConflicts = true,
                ConflictingVvns = conflictingVvns
            };
        }

        public async Task<VVNDto> RejectVvnAsync(Guid id, string rejectionReason, string officerId)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            vvn.Reject(rejectionReason, officerId);
            await _unitOfWork.CommitAsync();

            _apiLogger.LogVvnRejected(id, rejectionReason, officerId);

            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<VVNDto> ResubmitVvnAsync(Guid id)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            vvn.Resubmit();
            await _unitOfWork.CommitAsync();

            _apiLogger.LogVvnResubmitted(id);

            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<VVNDto> UpdateAndResubmitVvnAsync(Guid id, VVNSubmitDto dto)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            // Validate vessel exists
            var vessel = await _vesselRepo.GetByImoAsync(new IMOnumber(dto.ReferredVesselId))
                ?? throw new BusinessRuleValidationException($"Vessel with IMO '{dto.ReferredVesselId}' not found.");

            // Check for conflicting VVNs (exclude current VVN)
        
            var conflicts = await _repo.GetConflictingVvnsForVesselAsync(
                new IMOnumber(dto.ReferredVesselId),
                dto.ArrivalDate,
                dto.DepartureDate,
                vvn.Id);
                
                if (conflicts.Any())
                    {
                    var conflictDetails = conflicts.Select(c => 
                        $"VVN {c.Id.AsGuid()} from {c.ArrivalDate?.Value:yyyy-MM-dd HH:mm} to {c.DepartureDate?.Value:yyyy-MM-dd HH:mm}");
                    throw new BusinessRuleValidationException(
                        $"Vessel with IMO '{dto.ReferredVesselId}' already has conflicting visit notification(s) during this time period: {string.Join(", ", conflictDetails)}");
                }

            // Update dates and temporarily set to InProgress
            vvn.UpdateAndResubmit(dto.ArrivalDate, dto.DepartureDate);

            // Build manifests
            CargoManifest? loadingManifest = null;
            CargoManifest? unloadingManifest = null;

            if (dto.LoadingManifest != null && dto.LoadingManifest.Entries.Any())
            {
                loadingManifest = new CargoManifest(new ManifestType(ManifestTypeEnum.Load));
                foreach (var entryDto in dto.LoadingManifest.Entries)
                {
                    var container = await _containerRepo.GetByIdAsync(new ContainerId(Guid.Parse(entryDto.ContainerId)))
                        ?? throw new BusinessRuleValidationException($"Container with ID '{entryDto.ContainerId}' not found.");
                    
                    var sourceArea = await _storageAreaRepo.GetByIdAsync(new StorageAreaId(Guid.Parse(entryDto.SourceStorageAreaId!)))
                        ?? throw new BusinessRuleValidationException($"Storage area with ID '{entryDto.SourceStorageAreaId}' not found.");

                    var entry = ManifestEntry.CreateLoadEntry(
                        new ContainerId(Guid.Parse(entryDto.ContainerId)),
                        new StorageAreaId(Guid.Parse(entryDto.SourceStorageAreaId!)));
                    loadingManifest.AddEntry(entry);
                }
            }

            if (dto.UnloadingManifest != null && dto.UnloadingManifest.Entries.Any())
            {
                unloadingManifest = new CargoManifest(new ManifestType(ManifestTypeEnum.Unload));
                foreach (var entryDto in dto.UnloadingManifest.Entries)
                {
                    var container = await _containerRepo.GetByIdAsync(new ContainerId(Guid.Parse(entryDto.ContainerId)))
                        ?? throw new BusinessRuleValidationException($"Container with ID '{entryDto.ContainerId}' not found.");
                    
                    var targetArea = await _storageAreaRepo.GetByIdAsync(new StorageAreaId(Guid.Parse(entryDto.TargetStorageAreaId!)))
                        ?? throw new BusinessRuleValidationException($"Storage area with ID '{entryDto.TargetStorageAreaId}' not found.");

                    var entry = ManifestEntry.CreateUnloadEntry(
                        new ContainerId(Guid.Parse(entryDto.ContainerId)),
                        new StorageAreaId(Guid.Parse(entryDto.TargetStorageAreaId!)));
                    unloadingManifest.AddEntry(entry);
                }
            }

            // Update manifests
            vvn.UpdateManifestsForResubmit(loadingManifest, unloadingManifest);

            // Update hazardous status based on containers
            var allContainersForResubmit = await _containerRepo.GetAllAsync();
            vvn.UpdateHazardousStatus(allContainersForResubmit);

            // Now resubmit (validates and changes status to Submitted)
            vvn.Resubmit();

            await _unitOfWork.CommitAsync();

            _apiLogger.LogVvnResubmitted(id);

            return _mapper.Map<VVNDto>(vvn);
        }

        // Convert a rejected VVN back to draft and update it
        public async Task<VVNDraftDtoWId> ConvertRejectedToDraftAsync(Guid id, VVNDraftDto dto)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id))
                ?? throw new BusinessRuleValidationException("Vessel Visit Notification not found.");

            // Convert to draft (changes status from Rejected to InProgress)
            vvn.ConvertToDraft();

            // Update dates
            vvn.UpdateDates(dto.ArrivalDate, dto.DepartureDate);

            // Update manifests
            if (dto.LoadingManifest != null)
            {
                var loadManifest = await BuildCargoManifestAsync(dto.LoadingManifest, ManifestTypeEnum.Load, validateReferences: false);
                vvn.SetLoadingManifest(loadManifest);
            }
            else
            {
                vvn.RemoveLoadingManifest();
            }

            if (dto.UnloadingManifest != null)
            {
                var unloadManifest = await BuildCargoManifestAsync(dto.UnloadingManifest, ManifestTypeEnum.Unload, validateReferences: false);
                vvn.SetUnloadingManifest(unloadManifest);
            }
            else
            {
                vvn.RemoveUnloadingManifest();
            }

            // Update hazardous status based on containers
            var allContainers = await _containerRepo.GetAllAsync();
            vvn.UpdateHazardousStatus(allContainers);

            await _unitOfWork.CommitAsync();

            _apiLogger.LogVvnUpdated(id);

            return _mapper.Map<VVNDraftDtoWId>(vvn);
        }

        public async Task<List<VVNDto>> GetAllPendingAsync()
        {
            var vvns = await _repo.GetAllSubmittedAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<List<VVNDto>> GetAllSubmittedAsync()
        {
            var vvns = await _repo.GetAllSubmittedAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetSubmittedByIdAsync(Guid id)
        {
            var vvn = await _repo.GetSubmittedByIdAsync(new VesselVisitNotificationId(id)) ?? throw new InvalidOperationException($"Vessel Visit Notification with ID '{id}' not found.");
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<List<VVNDto>> GetAllReviewedAsync()
        {
            var vvns = await _repo.GetAllReviewedAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetReviewedByIdAsync(Guid id)
        {
            var vvn = await _repo.GetReviewedByIdAsync(new VesselVisitNotificationId(id)) ?? throw new InvalidOperationException($"Vessel Visit Notification with ID '{id}' not found.");
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<List<VVNDto>> GetAllDraftsAsync()
        {
            var vvns = await _repo.GetAllDraftsAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetDraftByIdAsync(Guid id)
        {
            var vvn = await _repo.GetDraftByIdAsync(new VesselVisitNotificationId(id)) ?? throw new InvalidOperationException($"Vessel Visit Notification with ID '{id}' not found.");
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task DeleteDraftAsync(Guid id)
        {
            await _repo.DeleteAsync(new VesselVisitNotificationId(id));
            await _unitOfWork.CommitAsync();
        }

        // Get all approved VVNs for a specific date (for scheduling)
        public async Task<List<VVNDto>> GetApprovedVVNsForDateAsync(DateTime date)
        {
            Console.WriteLine($"[VVNService] GetApprovedVVNsForDateAsync called with date: {date:yyyy-MM-dd}");
            var vvns = await _repo.GetAllApprovedForDateAsync(date);
            Console.WriteLine($"[VVNService] Repository returned {vvns.Count} VVNs");
            
            if (vvns.Any())
            {
                Console.WriteLine("[VVNService] VVN Details:");
                foreach (var vvn in vvns)
                {
                    Console.WriteLine($"  - VVN {vvn.Id}: ArrivalDate={vvn.ArrivalDate?.Value}, Status={vvn.StatusValue}");
                }
            }
            
            var result = _mapper.Map<List<VVNDto>>(vvns);
            Console.WriteLine($"[VVNService] Mapped to {result.Count} DTOs");
            return result;
        }
    }
}
