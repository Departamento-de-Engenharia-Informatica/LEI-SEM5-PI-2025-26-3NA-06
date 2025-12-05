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

            // Log unauthorized access attempts (403 Forbidden)
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var user = context.User;
                var email = user?.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
                var role = user?.FindFirstValue(ClaimTypes.Role) ?? "None";
                var path = context.Request.Path;
                var method = context.Request.Method;

                _logger.LogWarning(
                    "Unauthorized access attempt - User: {Email}, Role: {Role}, Method: {Method}, Path: {Path}",
                    email, role, method, path
                );
            }
        }
    }
}
