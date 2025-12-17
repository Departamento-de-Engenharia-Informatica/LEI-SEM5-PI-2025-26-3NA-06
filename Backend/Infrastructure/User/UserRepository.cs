using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Infrastructure.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Infrastructure
{
    public class UserRepository : BaseRepository<User, UserId>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context.Users)
        {
            _context = context;
        }

        public async Task<User?> FindByEmailAsync(Email email)
        {
           var entity = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return entity;
        }

        public async Task<User> GetUserByConfirmationTokenAsync(string token)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.ConfirmationToken.Equals(token));
            if (entity == null)
            {
                throw new InvalidOperationException($"User with confirmation token '{token}' not found.");
            }
            return entity;
        }


        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public IQueryable<User> GetQueryable()
        {
            return _context.Users.AsQueryable();
        }

        public async Task<User> GetUserByUsernameAsync(Username username)
        {
            var entity = await _context.Users
            .SingleOrDefaultAsync(u => u.Username.Value == username.Value);
            if (entity == null)
            {
                throw new InvalidOperationException($"User with username '{username.Value}' not found.");
            }
            return entity;
        }


        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
