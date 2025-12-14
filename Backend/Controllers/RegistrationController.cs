using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.Services;
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
        public async Task<IActionResult> SelfRegister([FromBody] UserUpsertDto dto)
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
                return Ok(new { message = "Account has been registered. Please wait for an administrator to activate your account." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("confirm-email")]
        public IActionResult ConfirmEmail(string token)
        {
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties 
            { 
                RedirectUri = $"/api/registration/complete-activation?token={token}",
                Items = { { "token", token } }
            };
            
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("complete-activation")]
        public async Task<IActionResult> CompleteActivation(string token)
        {
            try
            {
                // Get the authenticated user's email from Google OAuth
                var authenticatedEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Redirect("http://localhost:5173/access-denied?reason=Authentication required");
                }

                // Get the user associated with the token
                var userToActivate = await _registrationService.GetUserByTokenAsync(token);
                
                if (userToActivate == null)
                {
                    return Redirect("http://localhost:5173/access-denied?reason=Invalid or expired activation link");
                }

                // Verify that the authenticated email matches the user being activated
                if (userToActivate.Email.Value != authenticatedEmail)
                {
                    return Redirect($"http://localhost:5173/access-denied?reason=Email mismatch. You authenticated as {authenticatedEmail} but this activation link is for {userToActivate.Email.Value}");
                }

                // Activate the user
                await _registrationService.ConfirmEmailAsync(token);
                
                // User is now authenticated AND activated - redirect to appropriate dashboard based on role
                var role = userToActivate.Role.Value.ToString();
                var dashboardUrl = role switch
                {
                    "Admin" => "http://localhost:5173/admin",
                    "PortAuthorityOfficer" => "http://localhost:5173/port-authority",
                    "LogisticOperator" => "http://localhost:5173/logistic-operator",
                    "ShippingAgentRepresentative" => "http://localhost:5173/shipping-agent",
                    _ => "http://localhost:5173/login"
                };
                
                return Redirect($"{dashboardUrl}?role={role}");
            }
            catch (Exception ex)
            {
                return Redirect($"http://localhost:5173/access-denied?reason={Uri.EscapeDataString(ex.Message)}");
            }
        }
    }
}
