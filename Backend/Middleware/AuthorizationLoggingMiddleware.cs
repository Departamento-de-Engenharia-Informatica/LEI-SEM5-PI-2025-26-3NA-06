using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProjArqsi.Middleware
{
    public class AuthorizationLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationLoggingMiddleware> _logger;

        public AuthorizationLoggingMiddleware(RequestDelegate next, ILogger<AuthorizationLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            var path = context.Request.Path.Value ?? "";

            // Don't log authentication/login endpoints (these are expected to redirect)
            if (path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/signin", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Log unauthorized access attempts (401 Unauthorized or 403 Forbidden)
            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                var method = context.Request.Method;

                _logger.LogWarning(
                    "UNAUTHORIZED ACCESS ATTEMPT - Method: {Method}, Path: {Path} - User not authenticated",
                    method, path
                );
            }
            else if (statusCode == StatusCodes.Status403Forbidden)
            {
                var user = context.User;
                var email = user?.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
                var role = user?.FindFirstValue(ClaimTypes.Role) ?? "None";
                var method = context.Request.Method;

                _logger.LogWarning(
                    "FORBIDDEN ACCESS ATTEMPT - User: {Email}, Role: {Role}, Method: {Method}, Path: {Path} - Insufficient permissions",
                    email, role, method, path
                );
            }
            // Log redirects to login (302 redirect means authentication required)
            else if (statusCode == StatusCodes.Status302Found || statusCode == StatusCodes.Status307TemporaryRedirect)
            {
                var location = context.Response.Headers["Location"].ToString();
                if (location.Contains("/login", StringComparison.OrdinalIgnoreCase) || 
                    location.Contains("Account/Login", StringComparison.OrdinalIgnoreCase) ||
                    location.Contains("google", StringComparison.OrdinalIgnoreCase))
                {
                    var method = context.Request.Method;
                    
                    _logger.LogWarning(
                        "UNAUTHORIZED ACCESS ATTEMPT - Method: {Method}, Path: {Path} - Redirected to login (not authenticated)",
                        method, path
                    );
                }
            }
        }
    }
}
