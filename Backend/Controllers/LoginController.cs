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
            if (needsRegistration == "true")
            {
                return Redirect("http://localhost:5173/register");
            }
            return Redirect("http://localhost:5173");
        }

        [HttpPost("api/weblogin")]
        public async Task<IActionResult> WebLogin([FromBody] TokenRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.token);
                var emailGoogle = payload.Email;

                var userDto = await userService.GetUserByUsernameAsync(emailGoogle);
                                
                if (userDto == null || !userDto.IsActive)
                {
                    Console.WriteLine("IAM email not found or inactive. Checking personal email.");
                    userDto = await userService.checkIfAccountExists(emailGoogle);

                    if (userDto == null || !userDto.IsActive)
                    {
                        Console.WriteLine("Personal email not found or inactive.");
                        Console.WriteLine("Login failed: email not found.");

                        var dataProtectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                        var protector = dataProtectionProvider.CreateProtector("CustomCookieProtector");
                        var encryptedEmail = protector.Protect(emailGoogle);

                        Console.WriteLine("Creating CustomCookie before any SignInAsync calls.");

                        HttpContext.Response.Cookies.Append(".AspNetCore.CustomCookies", encryptedEmail, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                            SameSite = SameSiteMode.None,
                            Path = "/"
                        });

                        Console.WriteLine("CustomCookie has been appended to response.");

                        return StatusCode(302, new { 
                            Message = "This email is not registered in the system."
                        });
                    }

                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, emailGoogle),
                    new Claim(ClaimTypes.Role, userDto.Role.ToString()),
                    new Claim("Active", userDto.IsActive ? "1" : "0"),
                    new Claim("UserId", userDto.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                Console.WriteLine("Calling SignInAsync for authenticated user.");

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                
                return Ok(new { 
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Token validation error", Error = ex.Message });
            }
        }


        [HttpGet("api/claims")]
        public IActionResult GetClaims()
        {
            if(User.Identity == null)
            {
                return Unauthorized(new { Message = "Usuário não autenticado" });
            }
            if (User.Identity.IsAuthenticated)
            {
                var claims = User.Claims;

                var claimsData = new List<object>();
                foreach (var claim in claims)
                {
                    claimsData.Add(new
                    {
                        claim.Type,
                        claim.Value
                    });
                }

                return Ok(claimsData);
            }
            else
            {
                return Unauthorized(new { Message = "Usuário não autenticado" });
            }
        }
    }
}

public class TokenRequest
{
    public string? token { get; set; }
}