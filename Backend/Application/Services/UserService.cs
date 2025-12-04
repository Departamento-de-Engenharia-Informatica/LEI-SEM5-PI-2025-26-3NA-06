using AutoMapper;
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
        
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _mapper = mapper;
        }



        public async Task<List<UserDto>> GetAllAsync()
        {
            var list = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(list);
        }

        public async Task<UserDto?> GetByIdAsync(UserId id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AddAsync(CreateUserDto dto)
        {

            var existingUser = await _userRepository.FindByEmailAsync(new Email(dto.Email));

            if (existingUser != null)
            {
                throw new BusinessRuleValidationException("Email já existe no sistema, por favor tente novamente com outro email.");
            }

            var roleType = Enum.Parse<RoleType>(dto.Role);
            var user = new User(new Username(dto.Username), new Role(roleType), new Email(dto.Email));

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateAsync(UserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(dto.Id));
            if (user == null) return null;

            user.ChangeRole(dto.Role);
            user.ChangeUsername(dto.Username);
            user.ChangeEmail(dto.Email);
            user.ChangeConfirmationToken(dto.ConfirmationToken);

            await _unitOfWork.CommitAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> DeleteAsync(UserId id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            if (user.IsActive)
            {
                throw new BusinessRuleValidationException("Não é possível excluir um usuário ativo.");
            }

            await _userRepository.DeleteAsync(user.Id);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> DeleteFailureAsync(UserId id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            await _userRepository.DeleteAsync(user.Id);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(new Email(email));
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> FindByConfirmationTokenAsync(string token)
        {
            var user = await _userRepository.GetUserByConfirmationTokenAsync(token);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(new Username(username));
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> checkIfAccountExists(string email)
        {
            var user = await _userRepository.FindByEmailAsync(new Email(email));
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

    }
}
