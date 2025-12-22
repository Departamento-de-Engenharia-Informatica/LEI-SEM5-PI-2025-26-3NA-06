using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ILogger<AuditController> _logger;

        public AuditController(ILogger<AuditController> logger)
        {
            _logger = logger;
        }

        [HttpPost("unauthorized-access")]
        [AllowAnonymous]
        public IActionResult LogUnauthorizedAccess([FromBody] UnauthorizedAccessLogDto dto)
        {            
            _logger.LogWarning(
                "ACCESS DENIED - User: {Email}, Role: {Role}, Path: {Path}, Required: {RequiredRole}, Timestamp: {Timestamp}",
                dto.Email, dto.Role, dto.AttemptedPath, dto.RequiredRole, dto.Timestamp);

            return Ok(new { message = "Unauthorized access logged", email = dto.Email, path = dto.AttemptedPath });
        }
    }

    public class UnauthorizedAccessLogDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AttemptedPath { get; set; } = string.Empty;
        public string RequiredRole { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}
