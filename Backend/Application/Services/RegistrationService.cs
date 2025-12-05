using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Application.Services
{
    public class RegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationService(IUnitOfWork unitOfWork, IUserRepository userRepository, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task SelfRegisterUserAsync(CreateUserDto dto, string iamEmail)
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

            // DO NOT send activation email yet - wait for admin to assign role and approve
            // Activation email will be sent when admin assigns role via US 3.2.5
        }
        
        public async Task ConfirmEmailAsync(string token)
        {
            // after User clicks the link in the email, we activate him
            var user = await _userRepository.GetUserByConfirmationTokenAsync(token);
            
            if (user == null)
            {
                throw new Exception("Invalid token or email.");
            }

            // Activate the user account
            user.Activate();
            user.ConfirmationToken = string.Empty;
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<User?> GetUserByTokenAsync(string token)
        {
            return await _userRepository.GetUserByConfirmationTokenAsync(token);
        }

        public async Task RegisterUserAsync(User user)
        {
            user.GenerateConfirmationToken();
            user.Deactivate();
            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
    
        }

        public async Task<User?> FindByEmailAsync(Email email)
        {
            return await _userRepository.FindByEmailAsync(email);
        }
    
        private async Task<User> updateUser(User user, CreateUserDto dto, string iamEmail)
        {
            user.GenerateConfirmationToken();
            user.Deactivate();
            user.ChangeUsername(new Username(iamEmail));
            await _userRepository.UpdateUserAsync(user);
            return user;
        }
    }
}
