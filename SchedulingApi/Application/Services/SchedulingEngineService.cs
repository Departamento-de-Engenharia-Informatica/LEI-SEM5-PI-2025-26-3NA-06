using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Logging;

namespace ProjArqsi.SchedulingApi.Services
{
    public interface ISchedulingEngine
    {
        Task<DailyScheduleResultDto> GenerateDailyScheduleAsync(
            DateTime targetDate,
            List<VesselVisitNotificationDto> vvns,
            List<DockDto> availableDocks,
            string accessToken);
    }

    public class SchedulingEngineService : ISchedulingEngine
    {
        private readonly ICoreApiClient _coreApiClient;
        private readonly ISchedulingLogger _schedulingLogger;

        public SchedulingEngineService(
            ICoreApiClient coreApiClient,
            ISchedulingLogger schedulingLogger)
        {
            _coreApiClient = coreApiClient;
            _schedulingLogger = schedulingLogger;
        }

        public async Task<DailyScheduleResultDto> GenerateDailyScheduleAsync(
            DateTime targetDate,
            List<VesselVisitNotificationDto> vvns,
            List<DockDto> availableDocks,
            string accessToken)
        {
            var result = new DailyScheduleResultDto
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                IsFeasible = true,
                Warnings = new List<string>(),
                Assignments = new List<DockAssignmentDto>()
            };

            if (!vvns.Any())
            {
                _schedulingLogger.LogInformation($"No VVNs to schedule for {targetDate.ToShortDateString()}");
                return result;
            }

            if (!availableDocks.Any())
            {
                _schedulingLogger.LogNoDocksAvailable(targetDate);
                result.IsFeasible = false;
                result.Warnings.Add("No docks available for assignment");
                return result;
            }

            // Step 1: Sort VVNs by ETA (ascending) for deterministic output
            var sortedVvns = vvns
                .OrderBy(v => v.ArrivalDate ?? DateTime.MaxValue)
                .ThenBy(v => v.Id) // Secondary sort by ID for full determinism
                .ToList();

            _schedulingLogger.LogSortingVvns(sortedVvns.Count);

            // Track dock assignments to detect time conflicts
            var dockSchedule = new Dictionary<Guid, List<DockAssignmentDto>>();
            foreach (var dock in availableDocks)
            {
                dockSchedule[dock.Id] = new List<DockAssignmentDto>();
            }

            // Step 2: Process each VVN
            foreach (var vvn in sortedVvns)
            {
                try
                {
                    var assignment = await ProcessVvnAsync(vvn, dockSchedule, availableDocks, targetDate, accessToken, result);
                    
                    if (assignment != null)
                    {
                        result.Assignments.Add(assignment);
                        
                        // Add to dock schedule tracking
                        if (assignment.DockId != Guid.Empty && dockSchedule.ContainsKey(assignment.DockId))
                        {
                            dockSchedule[assignment.DockId].Add(assignment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _schedulingLogger.LogError("ProcessVvnAsync", ex, new Dictionary<string, object> { { "VvnId", vvn.Id } });
                    result.Warnings.Add($"VVN {vvn.Id}: Failed to process - {ex.Message}");
                    result.IsFeasible = false;
                }
            }

            // Sort final assignments by ETA for consistent output
            result.Assignments = result.Assignments.OrderBy(a => a.Eta).ThenBy(a => a.VvnId).ToList();

            _schedulingLogger.LogInformation(
                $"Schedule generation complete: {result.Assignments.Count} assignments, {result.Warnings.Count} warnings, Feasible: {result.IsFeasible}");

            return result;
        }

        private async Task<DockAssignmentDto?> ProcessVvnAsync(
            VesselVisitNotificationDto vvn,
            Dictionary<Guid, List<DockAssignmentDto>> dockSchedule,
            List<DockDto> availableDocks,
            DateTime targetDate,
            string accessToken,
            DailyScheduleResultDto result)
        {
            // Fetch vessel details
            var vessel = await _coreApiClient.GetVesselByImoAsync(vvn.ReferredVesselId, accessToken);

            // Determine time window
            var eta = vvn.ArrivalDate ?? targetDate;
            var etd = vvn.DepartureDate ?? eta.AddHours(6); // Default 6-hour window

            if (!vvn.DepartureDate.HasValue)
            {
                result.Warnings.Add($"VVN {vvn.Id}: No departure date specified, using default 6-hour window");
            }

            // Check if VVN already has a dock assigned by PortAuthority
            Guid preassignedDockId = Guid.Empty;
            bool hasPreassignedDock = !string.IsNullOrEmpty(vvn.TempAssignedDockId) && 
                                      Guid.TryParse(vvn.TempAssignedDockId, out preassignedDockId);

            DockDto? assignedDock = null;
            Guid finalDockId = Guid.Empty;

            if (hasPreassignedDock)
            {
                // VVN has a preassigned dock - verify feasibility but do NOT reassign
                assignedDock = await _coreApiClient.GetDockByIdAsync(preassignedDockId, accessToken);
                
                if (assignedDock == null)
                {
                    result.Warnings.Add($"VVN {vvn.Id}: Preassigned dock {preassignedDockId} not found");
                    result.IsFeasible = false;
                }
                else
                {
                    finalDockId = assignedDock.Id;
                    
                    // Verify no time conflicts with existing assignments on this dock
                    if (dockSchedule.ContainsKey(finalDockId))
                    {
                        var conflicts = DetectTimeConflicts(eta, etd, dockSchedule[finalDockId]);
                        
                        if (conflicts.Any())
                        {
                            result.IsFeasible = false;
                            result.Warnings.Add($"VVN {vvn.Id}: Time conflict detected on preassigned dock {assignedDock.DockName} with VVN(s): {string.Join(", ", conflicts)}");
                        }
                    }
                }
            }
            else
            {
                // No preassigned dock - find a suitable dock without conflicts
                assignedDock = FindAvailableDock(eta, etd, availableDocks, dockSchedule);
                
                if (assignedDock == null)
                {
                    result.IsFeasible = false;
                    result.Warnings.Add($"VVN {vvn.Id}: No available dock found without time conflicts");
                }
                else
                {
                    finalDockId = assignedDock.Id;
                    _schedulingLogger.LogVvnAssignedToDock(vvn.Id, assignedDock.Id, assignedDock.DockName);
                }
            }

            // Create assignment (even if no dock found, for reporting)
            var assignment = new DockAssignmentDto
            {
                VvnId = vvn.Id,
                VesselId = vessel?.Id ?? Guid.Empty,
                VesselImo = vessel?.Imo ?? vvn.ReferredVesselId,
                VesselName = vessel?.VesselName,
                DockId = finalDockId,
                DockName = assignedDock?.DockName,
                Eta = eta,
                Etd = etd,
                EstimatedTeu = vvn.EstimatedTeu
            };

            if (vessel == null)
            {
                result.Warnings.Add($"VVN {vvn.Id}: Vessel details not found for IMO {vvn.ReferredVesselId}");
            }

            return assignment;
        }

        /// <summary>
        /// Detects time window conflicts between a new assignment and existing assignments.
        /// Two time windows conflict if they overlap: [eta1, etd1] overlaps [eta2, etd2] if eta1 < etd2 AND eta2 < etd1
        /// </summary>
        private List<Guid> DetectTimeConflicts(DateTime eta, DateTime etd, List<DockAssignmentDto> existingAssignments)
        {
            var conflicts = new List<Guid>();

            foreach (var existing in existingAssignments)
            {
                // Check for time overlap: [eta, etd] overlaps [existing.Eta, existing.Etd]
                bool overlaps = eta < existing.Etd && existing.Eta < etd;
                
                if (overlaps)
                {
                    conflicts.Add(existing.VvnId);
                    _schedulingLogger.LogWarning(
                        $"Time conflict detected: [{eta}, {etd}] overlaps with VVN {existing.VvnId} [{existing.Eta}, {existing.Etd}]");
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Finds the first available dock without time conflicts.
        /// Returns null if no dock is available.
        /// </summary>
        private DockDto? FindAvailableDock(
            DateTime eta,
            DateTime etd,
            List<DockDto> availableDocks,
            Dictionary<Guid, List<DockAssignmentDto>> dockSchedule)
        {
            // Try each dock in order (deterministic)
            foreach (var dock in availableDocks.OrderBy(d => d.DockName))
            {
                if (!dockSchedule.ContainsKey(dock.Id))
                {
                    continue; // Should not happen, but safety check
                }

                var existingAssignments = dockSchedule[dock.Id];
                var conflicts = DetectTimeConflicts(eta, etd, existingAssignments);

                if (!conflicts.Any())
                {
                    // No conflicts - this dock is available
                    return dock;
                }
            }

            return null; // No available dock found
        }
    }
}
