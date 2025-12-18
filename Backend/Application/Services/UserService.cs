using AutoMapper;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, EmailService emailService, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _emailService = emailService;
            _mapper = mapper;
        }



        public async Task<List<UserDto>> GetAllAsync()
        {
            var list = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(list);
        }

        public async Task<List<UserDto>> GetInactiveUsersAsync()
        {
            var list = await _userRepository.GetAllAsync();
            var inactiveUsers = list.Where(u => !u.IsActive).ToList();
            return _mapper.Map<List<UserDto>>(inactiveUsers);
        }

        public async Task<UserDto?> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(new Email(email));
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> ToggleUserActiveAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(id)) ?? throw new KeyNotFoundException($"User with id '{id}' not found.");
            if (user.IsActive)
            {
                user.Deactivate();
            }
            else
            {
                user.Activate();
            }

            await _unitOfWork.CommitAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AssignRoleAndSendActivationEmailAsync(Guid id, RoleType role)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(id)) ?? throw new KeyNotFoundException($"User with id '{id}' not found.");

            // Update role
            user.ChangeRole(new Role(role));
            
            // Ensure confirmation token exists
            if (string.IsNullOrEmpty(user.ConfirmationToken))
            {
                user.GenerateConfirmationToken();
            }
            
            await _unitOfWork.CommitAsync();

            // Send activation email
            await _emailService.SendConfirmationEmailAsync(
                user,
                user.Email.Value,
                user.ConfirmationToken
            );

            return _mapper.Map<UserDto>(user);
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

    }
}
