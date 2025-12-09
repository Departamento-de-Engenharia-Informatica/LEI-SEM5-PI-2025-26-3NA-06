using Microsoft.AspNetCore.Mvc;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(ILogger<AuditLogController> logger)
        {
            _logger = logger;
        }

        [HttpPost("unauthorized-access")]
        public IActionResult LogUnauthorizedAccess([FromBody] UnauthorizedAccessLog log)
        {
            _logger.LogWarning(
                "UNAUTHORIZED ACCESS ATTEMPT - User: {Name}, Email: {Email}, Role: {Role}, Attempted Route: {Route}",
                log.Name ?? "Unknown",
                log.Email ?? "Unknown",
                log.Role ?? "None",
                log.AttemptedRoute ?? "Unknown"
            );

            return Ok();
        }
    }

    public class UnauthorizedAccessLog
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? AttemptedRoute { get; set; }
    }
}
