using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.Services;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly RegistrationService _registrationService;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            RegistrationService registrationService,
            ILogger<RegistrationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserUpsertDto dto)
        {
            try
            {
                _logger.LogInformation("Registration request for email: {Email}", dto.Email);

                await _registrationService.SelfRegisterUserAsync(dto, dto.Email);

                return Ok(new 
                { 
                    success = true,
                    message = "Registration successful! Please wait for an administrator to approve your account and assign you a role."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", dto.Email);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
