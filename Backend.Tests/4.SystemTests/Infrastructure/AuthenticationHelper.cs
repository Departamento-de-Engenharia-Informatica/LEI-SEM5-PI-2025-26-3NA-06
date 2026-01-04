using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Tests.SystemTests.Infrastructure;

/// <summary>
/// Helper class to generate JWT tokens for testing authentication scenarios.
/// </summary>
public static class AuthenticationHelper
{
    private const string SecretKey = "TestSecretKeyForJwtTokenGeneration123456789012345678901234567890"; // Min 32 chars (64 chars)
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    /// <summary>
    /// Generates a JWT token with specified roles for testing.
    /// </summary>
    /// <param name="userId">User ID to include in the token (default: test-user-123)</param>
    /// <param name="roles">Roles to assign to the user (e.g., "PortAuthorityOfficer", "ShippingAgent")</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes (default: 60)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateToken(string userId = "test-user-123", string[]? roles = null, int expiresInMinutes = 60)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles as claims
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a token with a single role.
    /// </summary>
    public static string GenerateToken(string role)
    {
        return GenerateToken(roles: new[] { role });
    }

    /// <summary>
    /// Generates an expired token for testing token expiration scenarios.
    /// </summary>
    public static string GenerateExpiredToken(string role = "PortAuthorityOfficer")
    {
        return GenerateToken(roles: new[] { role }, expiresInMinutes: -1);
    }
}
