namespace ProjArqsi.AuthApi.Logging
{
    public interface IAuthLogger
    {
        // Authentication
        void LogGoogleAuthenticationStarted(string credential);
        void LogGoogleAuthenticationSucceeded(string email, string googleId);
        void LogGoogleAuthenticationFailed(string error);
        void LogGoogleTokenValidationFailed(Exception ex);
        
        // User Creation
        void LogUserCreated(string userId, string email, string role);
        void LogUserCreationFailed(string email, string role, string error);
        
        // JWT Token Generation
        void LogJwtTokenGenerated(string userId, string email, string role);
        void LogJwtTokenGenerationFailed(string userId, Exception ex);
        
        // Email Verification
        void LogEmailVerificationTokenSent(string userId, string email);
        void LogEmailVerificationTokenSendFailed(string email, Exception ex);
        void LogEmailVerified(string userId, string email);
        void LogEmailVerificationFailed(string userId, string error);
        
        // Login/Logout
        void LogUserLoginAttempt(string email);
        void LogUserLoginSucceeded(string userId, string email, string role);
        void LogUserLoginFailed(string email, string reason);
        void LogUserLogout(string userId, string email);
        
        // Errors and Warnings
        void LogError(string operation, Exception ex, Dictionary<string, object>? context = null);
        void LogWarning(string message, Dictionary<string, object>? context = null);
        void LogInformation(string message, Dictionary<string, object>? context = null);
    }

    public class AuthLogger : IAuthLogger
    {
        private readonly ILogger<AuthLogger> _logger;

        public AuthLogger(ILogger<AuthLogger> logger)
        {
            _logger = logger;
        }

        // Authentication
        public void LogGoogleAuthenticationStarted(string credential)
        {
            _logger.LogInformation(
                "Google authentication started: CredentialLength={CredentialLength}",
                credential?.Length ?? 0);
        }

        public void LogGoogleAuthenticationSucceeded(string email, string googleId)
        {
            _logger.LogInformation(
                "Google authentication succeeded: Email={Email}, GoogleId={GoogleId}",
                email, googleId);
        }

        public void LogGoogleAuthenticationFailed(string error)
        {
            _logger.LogWarning(
                "Google authentication failed: Error={Error}",
                error);
        }

        public void LogGoogleTokenValidationFailed(Exception ex)
        {
            _logger.LogError(ex,
                "Google token validation failed");
        }

        // User Creation
        public void LogUserCreated(string userId, string email, string role)
        {
            _logger.LogInformation(
                "User created: UserId={UserId}, Email={Email}, Role={Role}",
                userId, email, role);
        }

        public void LogUserCreationFailed(string email, string role, string error)
        {
            _logger.LogError(
                "User creation failed: Email={Email}, Role={Role}, Error={Error}",
                email, role, error);
        }

        // JWT Token Generation
        public void LogJwtTokenGenerated(string userId, string email, string role)
        {
            _logger.LogInformation(
                "JWT token generated: UserId={UserId}, Email={Email}, Role={Role}",
                userId, email, role);
        }

        public void LogJwtTokenGenerationFailed(string userId, Exception ex)
        {
            _logger.LogError(ex,
                "JWT token generation failed: UserId={UserId}",
                userId);
        }

        // Email Verification
        public void LogEmailVerificationTokenSent(string userId, string email)
        {
            _logger.LogInformation(
                "Email verification token sent: UserId={UserId}, Email={Email}",
                userId, email);
        }

        public void LogEmailVerificationTokenSendFailed(string email, Exception ex)
        {
            _logger.LogError(ex,
                "Email verification token send failed: Email={Email}",
                email);
        }

        public void LogEmailVerified(string userId, string email)
        {
            _logger.LogInformation(
                "Email verified: UserId={UserId}, Email={Email}",
                userId, email);
        }

        public void LogEmailVerificationFailed(string userId, string error)
        {
            _logger.LogWarning(
                "Email verification failed: UserId={UserId}, Error={Error}",
                userId, error);
        }

        // Login/Logout
        public void LogUserLoginAttempt(string email)
        {
            _logger.LogInformation(
                "User login attempt: Email={Email}",
                email);
        }

        public void LogUserLoginSucceeded(string userId, string email, string role)
        {
            _logger.LogInformation(
                "User login succeeded: UserId={UserId}, Email={Email}, Role={Role}",
                userId, email, role);
        }

        public void LogUserLoginFailed(string email, string reason)
        {
            _logger.LogWarning(
                "User login failed: Email={Email}, Reason={Reason}",
                email, reason);
        }

        public void LogUserLogout(string userId, string email)
        {
            _logger.LogInformation(
                "User logout: UserId={UserId}, Email={Email}",
                userId, email);
        }

        // Errors and Warnings
        public void LogError(string operation, Exception ex, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogError(ex, "Error in operation: {Operation}", operation);
            }
            else
            {
                _logger.LogError(ex,
                    "Error in operation: {Operation}, Context: {@Context}",
                    operation, context);
            }
        }

        public void LogWarning(string message, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogWarning("{Message}, Context: {@Context}", message, context);
            }
        }

        public void LogInformation(string message, Dictionary<string, object>? context = null)
        {
            if (context == null || !context.Any())
            {
                _logger.LogInformation(message);
            }
            else
            {
                _logger.LogInformation("{Message}, Context: {@Context}", message, context);
            }
        }
    }
}
