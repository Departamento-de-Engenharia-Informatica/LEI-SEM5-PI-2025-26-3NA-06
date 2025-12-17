using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.Services;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivationController : ControllerBase
    {
        private readonly RegistrationService _registrationService;
        private readonly ILogger<ActivationController> _logger;

        public ActivationController(
            RegistrationService registrationService,
            ILogger<ActivationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Token is required" });
                }

                _logger.LogInformation("Activation attempt with token: {Token}", token);

                await _registrationService.ConfirmEmailAsync(token);

                _logger.LogInformation("User activated successfully with token: {Token}", token);

                return Ok(new 
                { 
                    success = true,
                    message = "Your account has been activated successfully! You can now log in." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user with token: {Token}", token);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
