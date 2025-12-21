using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.StorageAreaAggregate;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    // ManifestEntry is an owned entity within CargoManifest
    // It represents a single container movement intention
    public class ManifestEntry
    {
        public ContainerId ContainerId { get; private set; } = null!;
        
        // Source location: null means vessel (from VVN), otherwise storage area
        public StorageAreaId? SourceStorageAreaId { get; private set; }
        
        // Target location: null means vessel (from VVN), otherwise storage area
        public StorageAreaId? TargetStorageAreaId { get; private set; }

        protected ManifestEntry() { }

        // For Load manifest: container from storage area to vessel
        public static ManifestEntry CreateLoadEntry(ContainerId containerId, StorageAreaId sourceStorageAreaId)
        {
            if (containerId == null)
                throw new BusinessRuleValidationException("Container ID is required.");
            if (sourceStorageAreaId == null)
                throw new BusinessRuleValidationException("Source storage area is required for loading operations.");

            return new ManifestEntry
            {
                ContainerId = containerId,
                SourceStorageAreaId = sourceStorageAreaId,
                TargetStorageAreaId = null // Target is vessel (implicit from VVN)
            };
        }

        // For Unload manifest: container from vessel to storage area
        public static ManifestEntry CreateUnloadEntry(ContainerId containerId, StorageAreaId targetStorageAreaId)
        {
            if (containerId == null)
                throw new BusinessRuleValidationException("Container ID is required.");
            if (targetStorageAreaId == null)
                throw new BusinessRuleValidationException("Target storage area is required for unloading operations.");

            return new ManifestEntry
            {
                ContainerId = containerId,
                SourceStorageAreaId = null, // Source is vessel (implicit from VVN)
                TargetStorageAreaId = targetStorageAreaId
            };
        }

        public void ValidateForManifestType(ManifestType manifestType)
        {
            if (manifestType.Value == ManifestTypeEnum.Load)
            {
                if (SourceStorageAreaId == null)
                    throw new BusinessRuleValidationException("Load manifest entries must have a source storage area.");
                if (TargetStorageAreaId != null)
                    throw new BusinessRuleValidationException("Load manifest entries should not have a target storage area (target is vessel).");
            }
            else if (manifestType.Value == ManifestTypeEnum.Unload)
            {
                if (TargetStorageAreaId == null)
                    throw new BusinessRuleValidationException("Unload manifest entries must have a target storage area.");
                if (SourceStorageAreaId != null)
                    throw new BusinessRuleValidationException("Unload manifest entries should not have a source storage area (source is vessel).");
            }
        }
    }
}
