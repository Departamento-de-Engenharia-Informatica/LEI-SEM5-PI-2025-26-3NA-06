using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Services;
using ProjArqsi.SchedulingApi.Logging;
using System.Security.Claims;

namespace ProjArqsi.SchedulingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "LogisticOperator")]
    public class SchedulingController : ControllerBase
    {
        private readonly ICoreApiClient _coreApiClient;
        private readonly ISchedulingEngine _schedulingEngine;
        private readonly ISchedulingLogger _schedulingLogger;

        public SchedulingController(
            ICoreApiClient coreApiClient,
            ISchedulingEngine schedulingEngine,
            ISchedulingLogger schedulingLogger)
        {
            _coreApiClient = coreApiClient;
            _schedulingEngine = schedulingEngine;
            _schedulingLogger = schedulingLogger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ScheduleResponseDto>> GenerateSchedule([FromBody] ScheduleRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
                
                _schedulingLogger.LogScheduleGenerationStarted(request.TargetDate, userId, userEmail);

                // Get access token from current request to forward to Core API
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized(new { message = "Access token is required" });
                }

                // Fetch approved VVNs for the target date from Core API
                var vvns = await _coreApiClient.GetApprovedVVNsForDateAsync(request.TargetDate, accessToken);

                if (!vvns.Any())
                {
                    return Ok(new ScheduleResponseDto
                    {
                        TargetDate = request.TargetDate,
                        GeneratedAt = DateTime.UtcNow,
                        OperationPlans = new List<OperationPlanDto>()
                    });
                }

                // TODO: Implement scheduling algorithm logic
                // For now, return a simple placeholder response
                var response = new ScheduleResponseDto
                {
                    TargetDate = request.TargetDate,
                    GeneratedAt = DateTime.UtcNow,
                    OperationPlans = new List<OperationPlanDto>()
                };

                _schedulingLogger.LogScheduleGenerationCompleted(
                    request.TargetDate, vvns.Count, 0, 0, true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogScheduleGenerationFailed(request.TargetDate, ex);
                return StatusCode(500, new { message = "An error occurred while generating the schedule.", details = ex.Message });
            }
        }

        /// <summary>
        /// Generate a daily schedule for a specific date.
        /// Returns dock assignments for all approved VVNs on the target date.
        /// </summary>
        /// <param name="date">Target date in YYYY-MM-DD format</param>
        [HttpPost("daily")]
        public async Task<ActionResult<DailyScheduleResultDto>> GenerateDailySchedule([FromQuery] string date)
        {
            try
            {
                // Parse and validate date
                if (!DateTime.TryParse(date, out var targetDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD." });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";

                _schedulingLogger.LogScheduleGenerationStarted(targetDate, userId, userEmail);

                // Get access token from current request to forward to Core API
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized(new { message = "Access token is required" });
                }

                // Fetch approved VVNs for the target date from Core API
                var vvns = await _coreApiClient.GetApprovedVVNsForDateAsync(targetDate, accessToken);

                // Fetch all available docks
                var docks = await _coreApiClient.GetAllDocksAsync(accessToken);

                // Generate schedule using scheduling engine
                var result = await _schedulingEngine.GenerateDailyScheduleAsync(targetDate, vvns, docks, accessToken);

                _schedulingLogger.LogScheduleGenerationCompleted(
                    targetDate, vvns.Count, result.Assignments.Count, result.Warnings.Count, result.IsFeasible);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogError("GenerateDailySchedule", ex, new Dictionary<string, object> { { "Date", date } });
                return StatusCode(500, new { message = "An error occurred while generating the daily schedule.", details = ex.Message });
            }
        }
    }
}
