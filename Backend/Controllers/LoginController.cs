using Google.Apis.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using ProjArqsi.Services;

namespace DDDSample1.Presentation.Controllers
{
    public class LoginController : Controller
    {
        UserService userService;

        public LoginController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("api/login")]
        public async Task<IActionResult> Login()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/login/callback" };

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
            return new EmptyResult();
        }

        [HttpGet("api/login/callback")]
        public IActionResult LoginCallback()
        {
            var needsRegistration = User.FindFirstValue("needs_registration");
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (needsRegistration == "true")
            {
                return Redirect("http://localhost:5173/register");
            }
            
            // Store role in query parameter so frontend can save it
            var dashboardUrl = role switch
            {
                "Admin" => "http://localhost:5173/admin?role=Admin",
                "PortAuthorityOfficer" => "http://localhost:5173/port-authority?role=PortAuthorityOfficer",
                "LogisticOperator" => "http://localhost:5173/logistic-operator?role=LogisticOperator",
                "ShippingAgentRepresentative" => "http://localhost:5173/shipping-agent?role=ShippingAgentRepresentative",
                _ => "http://localhost:5173/login"
            };
            
            return Redirect(dashboardUrl);
        }
    }
}