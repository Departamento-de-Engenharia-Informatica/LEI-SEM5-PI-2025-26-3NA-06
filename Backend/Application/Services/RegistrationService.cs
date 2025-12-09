using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Application.Services
{
    public class RegistrationService
    {
        private readonly IUserRepository _userRepository;
        
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationService(IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task SelfRegisterUserAsync(UserUpsertDto dto, string iamEmail)
        {
            var existingUser = await _userRepository.FindByEmailAsync(new Email(iamEmail));
            if (existingUser != null)
            {
                throw new Exception("A user with this email already exists.");
            }

            // Create new user
            var roleType = Enum.Parse<RoleType>(dto.Role);
            var user = new User(new Username(dto.Username), new Role(roleType), new Email(iamEmail));
            user.GenerateConfirmationToken();
            user.Deactivate();
            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
        }
        
        public async Task ConfirmEmailAsync(string token)
        {
            // after User clicks the link in the email, we activate him
            var user = await _userRepository.GetUserByConfirmationTokenAsync(token);
            
            if (user == null)
            {
                throw new Exception("Invalid token or email.");
            }

            // Check if token has expired
            if (user.ConfirmationTokenExpiry.HasValue && user.ConfirmationTokenExpiry.Value < DateTime.UtcNow)
            {
                throw new Exception("This activation link has expired. Please contact an administrator to request a new activation link.");
            }

            // Activate the user account
            user.Activate();
            user.ConfirmationToken = string.Empty;
            user.ConfirmationTokenExpiry = null;
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<User> GetUserByTokenAsync(string token)
        {
            return await _userRepository.GetUserByConfirmationTokenAsync(token);
        }
    }
}
