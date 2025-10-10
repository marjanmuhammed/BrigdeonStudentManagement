using Bridgeon.Data;
using Bridgeon.Dtos.Mentor;
using Bridgeon.Models;
using Bridgeon.Repositeries.Mentor;
using Microsoft.EntityFrameworkCore;
using Bridgeon.DTOs;

namespace Bridgeon.Services.Mentor
{
    public class MentorService : IMentorService
    {
        private readonly IMentorRepository _repo;
        private readonly AppDbContext _context;
        private readonly ILogger<MentorService> _logger;

        public MentorService(IMentorRepository repo, AppDbContext context, ILogger<MentorService> logger)
        {
            _repo = repo;
            _context = context;
            _logger = logger;
        }

        public async Task AssignMenteesAsync(MentorAssignDto dto)
        {
            var mentor = await _repo.GetByIdAsync(dto.MentorId);
            if (mentor == null) throw new Exception("Mentor not found");
            if (mentor.Role != "Mentor" && mentor.Role != "Admin")
                throw new Exception("Selected user is not a mentor");

            await _repo.AssignMenteesAsync(dto.MentorId, dto.UserIds);
        }

        public async Task UnassignMenteesAsync(MentorUnassignDto dto)
        {
            var mentor = await _repo.GetByIdAsync(dto.MentorId);
            if (mentor == null) throw new Exception("Mentor not found");

            await _repo.UnassignMenteesAsync(dto.MentorId, dto.UserIds);
        }

        public async Task<IEnumerable<MentorMenteeDto>> GetMenteesForMentorAsync(int mentorId)
        {
            var mentees = await _repo.GetMenteesAsync(mentorId);

            return mentees.Select(u => new MentorMenteeDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                ProfileImageUrl = u.ProfileImageUrl
            });
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllMentorsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Mentor" && !u.IsBlocked)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        // ✅ NEW METHOD: get all students (for assignment list)
        public async Task<IEnumerable<MentorMenteeDto>> GetAllUsersWithMentorAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "User" || u.Role == "Student")
                .Include(u => u.Mentor) // ✅ ensures mentor info is loaded
                .Select(u => new MentorMenteeDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    ProfileImageUrl = u.ProfileImageUrl,
                    MentorId = u.MentorId,
                    MentorName = u.Mentor != null ? u.Mentor.FullName : null
                })
                .ToListAsync();
        }

    }
}
