using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Logging;
using System.Text.Json;

namespace ProjArqsi.SchedulingApi.Services
{
    public interface ISchedulingEngine
    {
        Task<DailyScheduleResultDto> GenerateDailySchedule(string date, string accessToken);
        Task<DailyScheduleResultDto> GenerateDailyScheduleAsync(
            DateTime targetDate,
            List<VesselVisitNotificationDto> vvns,
            List<DockDto> availableDocks,
            string accessToken);
    }

    public class SchedulingEngineService : ISchedulingEngine
    {
        private readonly ICoreApiClientService _coreApiClientService;

        public SchedulingEngineService(
            ICoreApiClientService coreApiClientService)
        {
            _coreApiClientService = coreApiClientService;
        }

        //este metodo dá fetch aos vvns aprovados e todos os docks.
        //Todos os VVNs já têm assigned docks
        //o Scheduling Module apenas verifica se há conflitos de tempo
        // Devolve se um OperationPlan é feasible ou não e warnings associados
        public async Task<DailyScheduleResultDto> GenerateDailySchedule(string date, string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");

            if (!DateTime.TryParse(date, out var targetDate))
                throw new ArgumentException("Invalid date format");

            // Fetch approved VVNs for the target date from Core API (all have preassigned docks)
            var vvns = await _coreApiClientService.GetApprovedVVNsForDateAsync(targetDate, accessToken);

            // Fetch all available docks for reference
            var docks = await _coreApiClientService.GetAllDocksAsync(accessToken);

            // Create Operation Plan (verify conflicts only, no dock assignment)
            var result = await GenerateDailyScheduleAsync(targetDate, vvns, docks, accessToken);
            return result;
        }

        /// <summary>
        /// Creates an Operation Plan - a sequence of VVNs with their preassigned docks for a specific day.
        /// Checks for time window conflicts on each dock.
        /// Sets IsFeasible = false if any conflicts exist.
        /// All VVNs already have assigned docks from PortAuthority approval process.
        /// </summary>
        public async Task<DailyScheduleResultDto> GenerateDailyScheduleAsync(
            DateTime targetDate,
            List<VesselVisitNotificationDto> vvns,
            List<DockDto> availableDocks,
            string accessToken)
        {
            // Create Operation Plan (result structure)
            var result = new DailyScheduleResultDto
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                IsFeasible = true,
                Warnings = [],
                DockSchedules = []
            };

            if (vvns.Count == 0) return result;

            if (availableDocks.Count == 0)
            {
                result.IsFeasible = false;
                result.Warnings.Add("No docks available for assignment");
                return result;
            }

            // Step 1: Sort VVNs by ETA (ascending) for deterministic output
            var sortedVvns = vvns
                .OrderBy(v => v.ArrivalDate ?? DateTime.MaxValue)
                .ThenBy(v => v.Id) // Secondary sort by ID for full determinism
                .ToList();


            // Track dock assignments to detect time conflicts
            var dockSchedule = new Dictionary<Guid, List<DockAssignmentDto>>();
            foreach (var dock in availableDocks)
            {
                dockSchedule[dock.Id] = [];
            }

            // Step 2: Process each VVN
            foreach (var vvn in sortedVvns)
            {
                try
                {
                    var assignment = await ProcessVvnAsync(vvn, dockSchedule, accessToken, result);
                    
                    if (assignment != null && assignment.DockId != Guid.Empty && dockSchedule.ContainsKey(assignment.DockId))
                    {
                        // Add to dock schedule tracking
                        dockSchedule[assignment.DockId].Add(assignment);
                    }
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"VVN {vvn.Id}: Failed to process - {ex.Message}");
                    result.IsFeasible = false;
                }
            }

            // Step 3: Group assignments by dock and create DockSchedules
            result.DockSchedules = dockSchedule
                .Where(kvp => kvp.Value.Count != 0) // Only include docks with assignments
                .Select(kvp =>
                {
                    var dock = availableDocks.FirstOrDefault(d => d.Id == kvp.Key);
                    return new DailyDockScheduleDto
                    {
                        DockId = kvp.Key,
                        DockName = dock?.DockName ?? "Unknown Dock",
                        Assignments = kvp.Value.OrderBy(a => a.Eta).ThenBy(a => a.VvnId).ToList()
                    };
                })
                .OrderBy(ds => ds.DockName)
                .ToList();

            // Log Operation Plan to server console for debugging
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine($"OPERATION PLAN - {result.Date}");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine($"Feasible: {result.IsFeasible}");
            Console.WriteLine($"Total Docks with VVNs: {result.DockSchedules.Count}");
            Console.WriteLine($"Total VVNs: {result.DockSchedules.Sum(ds => ds.Assignments.Count)}");
            Console.WriteLine($"Warnings: {result.Warnings.Count}");
            if (result.Warnings.Count != 0)
            {
                Console.WriteLine("\nWarnings:");
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
            }
            Console.WriteLine("\nDock Schedules:");
            foreach (var dockSched in result.DockSchedules)
            {
                Console.WriteLine($"\n  Dock: {dockSched.DockName} ({dockSched.Assignments.Count} VVN(s))");
                foreach (var assignment in dockSched.Assignments)
                {
                    Console.WriteLine($"    - VVN {assignment.VvnId}: {assignment.VesselName} ({assignment.VesselImo}) [{assignment.Eta:yyyy-MM-dd HH:mm} - {assignment.Etd:yyyy-MM-dd HH:mm}]");
                }
            }
            Console.WriteLine("=".PadRight(80, '=') + "\n");

            return result;
        }

        private async Task<DockAssignmentDto?> ProcessVvnAsync(
            VesselVisitNotificationDto vvn,
            Dictionary<Guid, List<DockAssignmentDto>> dockSchedule,
            string accessToken,
            DailyScheduleResultDto result)
        {
            // Fetch vessel details
            var vessel = await _coreApiClientService.GetVesselByImoAsync(vvn.ReferredVesselId, accessToken);

            // Determine time window (always present)
            var eta = vvn.ArrivalDate!.Value;
            var etd = vvn.DepartureDate!.Value;

            // All approved VVNs have a preassigned dock from PortAuthority
            Guid preassignedDockId = Guid.Parse(vvn.TempAssignedDockId!);

            // Fetch the preassigned dock details
            var assignedDock = await _coreApiClientService.GetDockByIdAsync(preassignedDockId, accessToken);
            
            var finalDockId = assignedDock!.Id;

            // Check for time conflicts with other VVNs on the same dock
            if (dockSchedule.ContainsKey(finalDockId))
            {
                var conflicts = DetectTimeConflicts(eta, etd, dockSchedule[finalDockId]);

                if (conflicts.Any())
                {
                    result.IsFeasible = false;
                    result.Warnings.Add($"VVN {vvn.Id}: Time window conflict on dock '{assignedDock.DockName}' with VVN(s): {string.Join(", ", conflicts)}");
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
                }
            }

            return conflicts;
        }
    }
}
