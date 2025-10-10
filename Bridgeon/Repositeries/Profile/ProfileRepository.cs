using Microsoft.EntityFrameworkCore;
using Bridgeon.Data;
using Bridgeon.Models;
using Bridgeon.Repositories.Interfaces;

namespace Bridgeon.Repositories.Implementations
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Profile> GetByUserIdAsync(int userId)
        {
            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Profile> CreateAsync(Profile profile)
        {
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<Profile> UpdateAsync(Profile profile)
        {
            profile.UpdatedAt = DateTime.UtcNow;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<bool> DeleteAsync(int profileId)
        {
            var profile = await _context.Profiles.FindAsync(profileId);
            if (profile != null)
            {
                _context.Profiles.Remove(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}