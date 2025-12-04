using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Domain.UserAggregate
{
    public interface IUserRepository : IRepository<User, UserId>
    {
        Task<User?> FindByEmailAsync(Email email);
        Task<User?> GetUserByConfirmationTokenAsync(string token);
        Task UpdateUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(Username username);
        Task DeleteUserAsync(User user);
    }
}
