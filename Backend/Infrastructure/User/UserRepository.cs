using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.Infrastructure.Shared;

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
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByConfirmationTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ConfirmationToken.Equals(token));
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

        public async Task<User?> GetUserByUsernameAsync(Username username)
        {
            return await _context.Users
            .SingleOrDefaultAsync(u => u.Username.Value == username.Value);
        }


        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        
    }
}
