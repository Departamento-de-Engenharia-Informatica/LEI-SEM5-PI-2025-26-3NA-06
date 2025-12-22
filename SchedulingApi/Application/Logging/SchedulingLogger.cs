namespace ProjArqsi.SchedulingApi.Logging
{
    public interface ISchedulingLogger
    {
        // Schedule Generation
        void LogScheduleGenerationStarted(DateTime targetDate, string userId, string userEmail);
        void LogScheduleGenerationCompleted(DateTime targetDate, int vvnCount, int assignmentCount, int warningsCount, bool isFeasible);
        void LogScheduleGenerationFailed(DateTime targetDate, Exception ex);
        
        // VVN Processing
        void LogVvnProcessingStarted(Guid vvnId, string vesselImo);
        void LogVvnAssignedToDock(Guid vvnId, Guid dockId, string dockName);
        void LogVvnNoAvailableDock(Guid vvnId, string vesselImo);
        void LogVvnTimeConflict(Guid vvnId, Guid dockId, string dockName, List<Guid> conflictingVvnIds);
        void LogVvnPreassignedDockVerified(Guid vvnId, Guid dockId, bool hasConflicts);
        void LogVvnProcessingFailed(Guid vvnId, Exception ex);
        
        // Core API Calls
        void LogCoreApiCallStarted(string endpoint, string method);
        void LogCoreApiCallSucceeded(string endpoint, int statusCode, int recordCount);
        void LogCoreApiCallFailed(string endpoint, int statusCode, string error);
        void LogCoreApiException(string endpoint, Exception ex);
        
        // Scheduling Algorithm
        void LogSortingVvns(int vvnCount);
        void LogDockConflictCheck(Guid dockId, DateTime eta, DateTime etd, int existingAssignments);
        void LogNoDocksAvailable(DateTime targetDate);
        
        // Warnings
        void LogWarning(string message, Dictionary<string, object>? context = null);
        
        // General Information
        void LogInformation(string message, Dictionary<string, object>? context = null);
        
        // Errors
        void LogError(string operation, Exception ex, Dictionary<string, object>? context = null);
    }

    public class SchedulingLogger : ISchedulingLogger
    {
        private readonly ILogger<SchedulingLogger> _logger;

        public SchedulingLogger(ILogger<SchedulingLogger> logger)
        {
            _logger = logger;
        }

        // Schedule Generation
        public void LogScheduleGenerationStarted(DateTime targetDate, string userId, string userEmail)
        {
            _logger.LogInformation(
                "Schedule generation started: TargetDate={TargetDate}, UserId={UserId}, UserEmail={UserEmail}",
                targetDate.ToShortDateString(), userId, userEmail);
        }

        public void LogScheduleGenerationCompleted(DateTime targetDate, int vvnCount, int assignmentCount, int warningsCount, bool isFeasible)
        {
            _logger.LogInformation(
                "Schedule generation completed: TargetDate={TargetDate}, VvnCount={VvnCount}, Assignments={AssignmentCount}, Warnings={WarningsCount}, IsFeasible={IsFeasible}",
                targetDate.ToShortDateString(), vvnCount, assignmentCount, warningsCount, isFeasible);
        }

        public void LogScheduleGenerationFailed(DateTime targetDate, Exception ex)
        {
            _logger.LogError(ex,
                "Schedule generation failed: TargetDate={TargetDate}",
                targetDate.ToShortDateString());
        }

        // VVN Processing
        public void LogVvnProcessingStarted(Guid vvnId, string vesselImo)
        {
            _logger.LogDebug(
                "Processing VVN: VvnId={VvnId}, VesselImo={VesselImo}",
                vvnId, vesselImo);
        }

        public void LogVvnAssignedToDock(Guid vvnId, Guid dockId, string dockName)
        {
            _logger.LogInformation(
                "VVN assigned to dock: VvnId={VvnId}, DockId={DockId}, DockName={DockName}",
                vvnId, dockId, dockName);
        }

        public void LogVvnNoAvailableDock(Guid vvnId, string vesselImo)
        {
            _logger.LogWarning(
                "No available dock found for VVN: VvnId={VvnId}, VesselImo={VesselImo}",
                vvnId, vesselImo);
        }

        public void LogVvnTimeConflict(Guid vvnId, Guid dockId, string dockName, List<Guid> conflictingVvnIds)
        {
            _logger.LogWarning(
                "Time conflict detected: VvnId={VvnId}, DockId={DockId}, DockName={DockName}, ConflictingVvns={ConflictingVvns}",
                vvnId, dockId, dockName, string.Join(", ", conflictingVvnIds));
        }

        public void LogVvnPreassignedDockVerified(Guid vvnId, Guid dockId, bool hasConflicts)
        {
            _logger.LogInformation(
                "Preassigned dock verified: VvnId={VvnId}, DockId={DockId}, HasConflicts={HasConflicts}",
                vvnId, dockId, hasConflicts);
        }

        public void LogVvnProcessingFailed(Guid vvnId, Exception ex)
        {
            _logger.LogError(ex,
                "VVN processing failed: VvnId={VvnId}",
                vvnId);
        }

        // Core API Calls
        public void LogCoreApiCallStarted(string endpoint, string method)
        {
            _logger.LogDebug(
                "Core API call started: Method={Method}, Endpoint={Endpoint}",
                method, endpoint);
        }

        public void LogCoreApiCallSucceeded(string endpoint, int statusCode, int recordCount)
        {
            _logger.LogDebug(
                "Core API call succeeded: Endpoint={Endpoint}, StatusCode={StatusCode}, RecordCount={RecordCount}",
                endpoint, statusCode, recordCount);
        }

        public void LogCoreApiCallFailed(string endpoint, int statusCode, string error)
        {
            _logger.LogError(
                "Core API call failed: Endpoint={Endpoint}, StatusCode={StatusCode}, Error={Error}",
                endpoint, statusCode, error);
        }

        public void LogCoreApiException(string endpoint, Exception ex)
        {
            _logger.LogError(ex,
                "Core API exception: Endpoint={Endpoint}",
                endpoint);
        }

        // Scheduling Algorithm
        public void LogSortingVvns(int vvnCount)
        {
            _logger.LogInformation(
                "Sorting VVNs by ETA: Count={VvnCount}",
                vvnCount);
        }

        public void LogDockConflictCheck(Guid dockId, DateTime eta, DateTime etd, int existingAssignments)
        {
            _logger.LogDebug(
                "Checking dock conflicts: DockId={DockId}, ETA={ETA}, ETD={ETD}, ExistingAssignments={ExistingAssignments}",
                dockId, eta, etd, existingAssignments);
        }

        public void LogNoDocksAvailable(DateTime targetDate)
        {
            _logger.LogWarning(
                "No docks available for scheduling: TargetDate={TargetDate}",
                targetDate.ToShortDateString());
        }

        // Warnings and General
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
    }
}
