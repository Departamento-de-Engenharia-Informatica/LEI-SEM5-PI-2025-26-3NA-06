using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Services;

namespace ProjArqsi.SchedulingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "LogisticOperator")]
    public class SchedulingController : ControllerBase
    {
        
        private readonly ISchedulingEngine _schedulingEngine;
        

        public SchedulingController(
            ISchedulingEngine schedulingEngine)
        {
            _schedulingEngine = schedulingEngine;
        }

        [HttpPost("daily")]
        public async Task<ActionResult<DailyScheduleResultDto>> GenerateDailySchedule([FromQuery] string date)
        {
            try
            {
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _schedulingEngine.GenerateDailySchedule(date, accessToken);
                // Get access token from current request to forward to Core API
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "An error occurred while generating the daily schedule.", details = ex.Message });
            }
        }
    }
}
