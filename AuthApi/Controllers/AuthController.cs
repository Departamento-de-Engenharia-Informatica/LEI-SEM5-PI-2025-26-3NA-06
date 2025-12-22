using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Auth.Common;
using ProjArqsi.AuthApi.Models;
using ProjArqsi.Infrastructure;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.AuthApi.Logging;

namespace ProjArqsi.AuthApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly IAuthLogger _authLogger;

    public AuthController(
        AppDbContext dbContext,
        JwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration,
        IAuthLogger authLogger)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
        _authLogger = authLogger;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        try
        {            
            _authLogger.LogGoogleAuthenticationStarted(request.IdToken);
            
            // Validate Google ID token
            var googleClientId = _configuration["GoogleClientId"]
                ?? throw new InvalidOperationException("GoogleClientId not configured");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (Exception ex)
            {
                _authLogger.LogGoogleTokenValidationFailed(ex);
                return BadRequest(new { message = "Invalid Google ID token" });
            }

            if (!payload.EmailVerified)
            {
                return BadRequest(new { message = "Email not verified by Google" });
            }

            // Find user by email
            var email = new Email(payload.Email);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
            
                return Unauthorized(new 
                { 
                    requiresRegistration = true,
                    email = payload.Email,
                    name = payload.Name,
                    message = "Please complete your registration."
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Your account is inactive. Please contact an administrator." });
            }

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(
                user.Id.AsGuid(), 
                user.Email.Value, 
                user.Role.Value.ToString());
            
            _authLogger.LogGoogleAuthenticationSucceeded(user.Email.Value, payload.Subject);
            
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? new JwtSettings();

            var response = new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = jwtSettings.ExpirationMinutes * 60,
                User = new UserInfo
                {
                    UserId = user.Id.AsGuid(),
                    Email = user.Email.Value,
                    Role = user.Role.Value.ToString(),
                    Name = user.Username.Value
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _authLogger.LogGoogleAuthenticationFailed(ex.Message);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }
}
