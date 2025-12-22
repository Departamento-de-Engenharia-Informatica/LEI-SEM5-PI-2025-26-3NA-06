namespace ProjArqsi.Application.Logging
{
    public interface ICoreApiLogger
    {
        // VVN Operations
        void LogVvnDrafted(Guid vvnId, string vesselImo);
        void LogVvnSubmitted(Guid vvnId, string vesselImo);
        void LogVvnApproved(Guid vvnId, Guid dockId, string officerId);
        void LogVvnRejected(Guid vvnId, string reason, string officerId);
        void LogVvnResubmitted(Guid vvnId);
        void LogVvnDeleted(Guid vvnId);
        void LogVvnUpdated(Guid vvnId);
        
        // Container Operations
        void LogContainerCreated(Guid containerId, string containerType);
        void LogContainerUpdated(Guid containerId);
        void LogContainerDeleted(Guid containerId);
        
        // Dock Operations
        void LogDockCreated(Guid dockId, string dockName);
        void LogDockUpdated(Guid dockId, string dockName);
        void LogDockDeleted(Guid dockId);
        
        // Storage Area Operations
        void LogStorageAreaCreated(Guid storageAreaId, string areaName);
        void LogStorageAreaUpdated(Guid storageAreaId, string areaName);
        void LogStorageAreaDeleted(Guid storageAreaId);
        
        // Vessel Operations
        void LogVesselCreated(Guid vesselId, string imo, string vesselName);
        void LogVesselUpdated(Guid vesselId, string imo);
        void LogVesselDeleted(Guid vesselId);
        
        // User Operations
        void LogUserRegistered(string userId, string email, string role);
        void LogUserEmailConfirmed(string userId, string email);
        void LogUserLogin(string userId, string email);
        void LogUserLogout(string userId);
        
        // Error Logging
        void LogError(string operation, Exception ex, Dictionary<string, object>? context = null);
        void LogWarning(string message, Dictionary<string, object>? context = null);
        
        // General Information
        void LogInformation(string message, Dictionary<string, object>? context = null);
    }

    public class CoreApiLogger : ICoreApiLogger
    {
        private readonly ILogger<CoreApiLogger> _logger;

        public CoreApiLogger(ILogger<CoreApiLogger> logger)
        {
            _logger = logger;
        }

        // VVN Operations
        public void LogVvnDrafted(Guid vvnId, string vesselImo)
        {
            _logger.LogInformation(
                "VVN Drafted: VvnId={VvnId}, VesselImo={VesselImo}",
                vvnId, vesselImo);
        }

        public void LogVvnSubmitted(Guid vvnId, string vesselImo)
        {
            _logger.LogInformation(
                "VVN Submitted: VvnId={VvnId}, VesselImo={VesselImo}",
                vvnId, vesselImo);
        }

        public void LogVvnApproved(Guid vvnId, Guid dockId, string officerId)
        {
            _logger.LogInformation(
                "VVN Approved: VvnId={VvnId}, AssignedDockId={DockId}, OfficerId={OfficerId}",
                vvnId, dockId, officerId);
        }

        public void LogVvnRejected(Guid vvnId, string reason, string officerId)
        {
            _logger.LogInformation(
                "VVN Rejected: VvnId={VvnId}, Reason={Reason}, OfficerId={OfficerId}",
                vvnId, reason, officerId);
        }

        public void LogVvnResubmitted(Guid vvnId)
        {
            _logger.LogInformation(
                "VVN Resubmitted: VvnId={VvnId}",
                vvnId);
        }

        public void LogVvnDeleted(Guid vvnId)
        {
            _logger.LogInformation(
                "VVN Deleted: VvnId={VvnId}",
                vvnId);
        }

        public void LogVvnUpdated(Guid vvnId)
        {
            _logger.LogInformation(
                "VVN Updated: VvnId={VvnId}",
                vvnId);
        }

        // Container Operations
        public void LogContainerCreated(Guid containerId, string containerType)
        {
            _logger.LogInformation(
                "Container Created: ContainerId={ContainerId}, Type={ContainerType}",
                containerId, containerType);
        }

        public void LogContainerUpdated(Guid containerId)
        {
            _logger.LogInformation(
                "Container Updated: ContainerId={ContainerId}",
                containerId);
        }

        public void LogContainerDeleted(Guid containerId)
        {
            _logger.LogInformation(
                "Container Deleted: ContainerId={ContainerId}",
                containerId);
        }

        // Dock Operations
        public void LogDockCreated(Guid dockId, string dockName)
        {
            _logger.LogInformation(
                "Dock Created: DockId={DockId}, DockName={DockName}",
                dockId, dockName);
        }

        public void LogDockUpdated(Guid dockId, string dockName)
        {
            _logger.LogInformation(
                "Dock Updated: DockId={DockId}, DockName={DockName}",
                dockId, dockName);
        }

        public void LogDockDeleted(Guid dockId)
        {
            _logger.LogInformation(
                "Dock Deleted: DockId={DockId}",
                dockId);
        }

        // Storage Area Operations
        public void LogStorageAreaCreated(Guid storageAreaId, string areaName)
        {
            _logger.LogInformation(
                "Storage Area Created: StorageAreaId={StorageAreaId}, AreaName={AreaName}",
                storageAreaId, areaName);
        }

        public void LogStorageAreaUpdated(Guid storageAreaId, string areaName)
        {
            _logger.LogInformation(
                "Storage Area Updated: StorageAreaId={StorageAreaId}, AreaName={AreaName}",
                storageAreaId, areaName);
        }

        public void LogStorageAreaDeleted(Guid storageAreaId)
        {
            _logger.LogInformation(
                "Storage Area Deleted: StorageAreaId={StorageAreaId}",
                storageAreaId);
        }

        // Vessel Operations
        public void LogVesselCreated(Guid vesselId, string imo, string vesselName)
        {
            _logger.LogInformation(
                "Vessel Created: VesselId={VesselId}, IMO={Imo}, VesselName={VesselName}",
                vesselId, imo, vesselName);
        }

        public void LogVesselUpdated(Guid vesselId, string imo)
        {
            _logger.LogInformation(
                "Vessel Updated: VesselId={VesselId}, IMO={Imo}",
                vesselId, imo);
        }

        public void LogVesselDeleted(Guid vesselId)
        {
            _logger.LogInformation(
                "Vessel Deleted: VesselId={VesselId}",
                vesselId);
        }

        // User Operations
        public void LogUserRegistered(string userId, string email, string role)
        {
            _logger.LogInformation(
                "User Registered: UserId={UserId}, Email={Email}, Role={Role}",
                userId, email, role);
        }

        public void LogUserEmailConfirmed(string userId, string email)
        {
            _logger.LogInformation(
                "User Email Confirmed: UserId={UserId}, Email={Email}",
                userId, email);
        }

        public void LogUserLogin(string userId, string email)
        {
            _logger.LogInformation(
                "User Login: UserId={UserId}, Email={Email}",
                userId, email);
        }

        public void LogUserLogout(string userId)
        {
            _logger.LogInformation(
                "User Logout: UserId={UserId}",
                userId);
        }

        // Error and Warning Logging
        public void LogError(string operation, Exception ex, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogError(ex, "Error in operation: {Operation}", operation);
            }
            else
            {
                _logger.LogError(ex, 
                    "Error in operation: {Operation}, Context: {@Context}", 
                    operation, context);
            }
        }

        public void LogWarning(string message, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogWarning("{Message}, Context: {@Context}", message, context);
            }
        }

        // General Information
        public void LogInformation(string message, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogInformation(message);
            }
            else
            {
                _logger.LogInformation("{Message}, Context: {@Context}", message, context);
            }
        }
    }
}
