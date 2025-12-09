using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace ProjArqsi.Controllers
{
    public class LoginController : Controller
    {
        

        public LoginController()
        {
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
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
                        
            if (needsRegistration == "true")
            {
                return Redirect("http://localhost:5173/register");
            }
            
            // URL encode the parameters
            var encodedEmail = Uri.EscapeDataString(email ?? "");
            var encodedName = Uri.EscapeDataString(name ?? "");
            
            // Store role, email, and name in query parameters so frontend can save them
            var dashboardUrl = role switch
            {
                "Admin" => $"http://localhost:5173/admin?role=Admin&email={encodedEmail}&name={encodedName}",
                "PortAuthorityOfficer" => $"http://localhost:5173/port-authority?role=PortAuthorityOfficer&email={encodedEmail}&name={encodedName}",
                "LogisticOperator" => $"http://localhost:5173/logistic-operator?role=LogisticOperator&email={encodedEmail}&name={encodedName}",
                "ShippingAgentRepresentative" => $"http://localhost:5173/shipping-agent?role=ShippingAgentRepresentative&email={encodedEmail}&name={encodedName}",
                _ => "http://localhost:5173/login"
            };            
            return Redirect(dashboardUrl);
        }
    }
}