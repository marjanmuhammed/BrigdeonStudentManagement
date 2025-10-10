using Microsoft.EntityFrameworkCore;
using Bridgeon.Data;
using Bridgeon.Models;
using Microsoft.AspNetCore.Identity;

namespace Bridgeon.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserProfileRepository(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsBlocked);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsBlocked);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = newPasswordHash;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

    }
}