using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Auth.Common;
using ProjArqsi.AuthApi.Models;
using ProjArqsi.Infrastructure;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.AuthApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext dbContext,
        JwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration,
        ILogger<AuthController> _logger)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
        this._logger = _logger;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        try
        {
            _logger.LogInformation("=== Google Auth Request Received ===");
            _logger.LogInformation("ID Token received (length): {Length}", request.IdToken?.Length ?? 0);
            
            // Validate Google ID token
            var googleClientId = _configuration["GoogleClientId"]
                ?? throw new InvalidOperationException("GoogleClientId not configured");

            _logger.LogInformation("Using Google Client ID: {ClientId}", googleClientId);

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                _logger.LogInformation("Token validated successfully for email: {Email}", payload.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED to validate Google ID token");
                return BadRequest(new { message = "Invalid Google ID token", detail = ex.Message });
            }

            if (!payload.EmailVerified)
            {
                _logger.LogWarning("Email not verified for: {Email}", payload.Email);
                return BadRequest(new { message = "Email not verified by Google" });
            }

            _logger.LogInformation("Email verified: {Email}", payload.Email);

            // Find user by email
            var email = new Email(payload.Email);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                // New user - don't create in DB yet, let frontend registration complete it
                _logger.LogInformation("New user detected (not yet registered): {Email}", payload.Email);
                
                // Return user info so frontend can show registration form with pre-filled data
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
                _logger.LogWarning("Login attempt by inactive user: {Email}", payload.Email);
                return Unauthorized(new { message = "Your account is inactive. Please contact an administrator." });
            }

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(
                user.Id.AsGuid(), 
                user.Email.Value, 
                user.Role.Value.ToString());
            
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

            _logger.LogInformation("User authenticated: {Email}, Role: {Role}", user.Email.Value, user.Role.Value);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }
}
