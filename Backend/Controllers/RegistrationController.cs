using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly RegistrationService _registrationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public RegistrationController(
            EmailService emailService, 
            RegistrationService registrationService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _emailService = emailService;
            _registrationService = registrationService;
            _dataProtectionProvider = dataProtectionProvider;
        }


        [HttpGet("get-pending-email")]
        public IActionResult GetPendingEmail()
        {
            var token = HttpContext.Request.Cookies[".AspNetCore.CustomCookies"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "No pending registration found" });
            }

            try
            {
                var protector = _dataProtectionProvider.CreateProtector("CustomCookieProtector");
                var email = protector.Unprotect(token);
                return Ok(new { email });
            }
            catch
            {
                return Unauthorized(new { message = "Invalid token" });
            }
        }

        [HttpPost("self-register")]
        public async Task<IActionResult> SelfRegister([FromBody] CreateUserDto dto)
        {
            try
            {
                var token = HttpContext.Request.Cookies[".AspNetCore.CustomCookies"];
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Please log in with Google first.");
                }

                var protector = _dataProtectionProvider.CreateProtector("CustomCookieProtector");
                var email = protector.Unprotect(token);
                await _registrationService.SelfRegisterUserAsync(dto, email);
                HttpContext.Response.Cookies.Delete(".AspNetCore.CustomCookies");
                return Ok(new { message = "Registration successful! Please check your email to confirm your account." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            try
            {
                await _registrationService.ConfirmEmailAsync(token);
                return Ok("Email confirmed successfully. Your registration is now complete.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Email confirmation failed: {ex.Message}");
            }
        }
    }
}
