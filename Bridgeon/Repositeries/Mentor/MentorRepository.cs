using Bridgeon.Data;
using Bridgeon.Models;
using Microsoft.EntityFrameworkCore;

namespace Bridgeon.Repositeries.Mentor
{
    public class MentorRepository : IMentorRepository
    {
        private readonly AppDbContext _context;

        public MentorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetMenteesAsync(int mentorId)
        {
            return await _context.Users
                .Where(u => u.MentorId == mentorId)
                .Include(u => u.Profile)
                .ToListAsync();
        }

        public async Task AssignMenteesAsync(int mentorId, IEnumerable<int> userIds)
        {
            var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
            foreach (var u in users)
            {
                u.MentorId = mentorId;
                u.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UnassignMenteesAsync(int mentorId, IEnumerable<int> userIds)
        {
            var users = await _context.Users.Where(u => userIds.Contains(u.Id) && u.MentorId == mentorId).ToListAsync();
            foreach (var u in users)
            {
                u.MentorId = null;
                u.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
