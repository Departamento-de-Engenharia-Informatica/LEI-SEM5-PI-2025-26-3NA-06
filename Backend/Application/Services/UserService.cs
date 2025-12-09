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

        public async Task<UserDto> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(new Email(email));
            if (user == null) throw new KeyNotFoundException($"User with email '{email}' not found.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> ToggleUserActiveAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(id));

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
            var user = await _userRepository.GetByIdAsync(new UserId(id));

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

    }
}
