using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.OemApi.Application.DTOs;
using System.Security.Claims;

namespace ProjArqsi.OemApi.Controllers
{
    [ApiController]
    [Route("api/oem")]
    [Authorize(Policy = "LogisticOperator")]
    public class OemHealthController : ControllerBase
    {
        

        public OemHealthController()
        {
        }

        /// <summary>
        /// Health check endpoint to verify OEM API is accessible and properly authenticated.
        /// Requires LogisticOperator role.
        /// </summary>
        /// <returns>Service health information and authenticated user details</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(OemHealthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetHealth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("username")?.Value ?? "unknown";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value ?? "unknown";
            
            // Extract roles from claims
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            var response = new OemHealthResponseDto
            {
                Service = "OEM",
                Status = "ok",
                UtcNow = DateTime.UtcNow,
                User = new UserInfoDto
                {
                    Id = userId,
                    Username = username,
                    Email = email,
                    Roles = roles
                }
            };
            return Ok(response);
        }
    }
}
