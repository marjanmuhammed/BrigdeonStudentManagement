using Bridgeon.Data;
using Bridgeon.Dtos;
using Bridgeon.Models;
using Microsoft.EntityFrameworkCore;

namespace Bridgeon.Repositories
{
    public class ReviewScoreRepository : IReviewScoreRepository
    {
        private readonly AppDbContext _context;

        public ReviewScoreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewScoreDto>> GetReviewScoresAsync()
        {
            return await _context.ReviewScores
                .Include(rs => rs.User)
                .Select(rs => new ReviewScoreDto
                {
                    Id = rs.Id,
                    UserId = rs.UserId,
                    UserName = rs.User.FullName,
                    Week = rs.Week,
                    ReviewDate = rs.ReviewDate,
                    ReviewerName = rs.ReviewerName,
                    AcademicScore = rs.AcademicScore,
                    ReviewScoreValue = rs.ReviewScoreValue,
                    TaskScore = rs.TaskScore,
                    TotalScore = rs.TotalScore,
                    CreatedAt = rs.CreatedAt,
                    CreatedBy = rs.CreatedBy
                })
                .OrderByDescending(rs => rs.Week)
                .ThenBy(rs => rs.UserName)
                .ToListAsync();
        }

        public async Task<ReviewScoreDto> GetReviewScoreByIdAsync(int id)
        {
            var reviewScore = await _context.ReviewScores
                .Include(rs => rs.User)
                .FirstOrDefaultAsync(rs => rs.Id == id);

            if (reviewScore == null) return null;

            return new ReviewScoreDto
            {
                Id = reviewScore.Id,
                UserId = reviewScore.UserId,
                UserName = reviewScore.User.FullName,
                Week = reviewScore.Week,
                ReviewDate = reviewScore.ReviewDate,
                ReviewerName = reviewScore.ReviewerName,
                AcademicScore = reviewScore.AcademicScore,
                ReviewScoreValue = reviewScore.ReviewScoreValue,
                TaskScore = reviewScore.TaskScore,
                TotalScore = reviewScore.TotalScore,
                CreatedAt = reviewScore.CreatedAt,
                CreatedBy = reviewScore.CreatedBy
            };
        }

        public async Task<IEnumerable<ReviewScoreDto>> GetReviewScoresByUserIdAsync(int userId)
        {
            return await _context.ReviewScores
                .Include(rs => rs.User)
                .Where(rs => rs.UserId == userId)
                .Select(rs => new ReviewScoreDto
                {
                    Id = rs.Id,
                    UserId = rs.UserId,
                    UserName = rs.User.FullName,
                    Week = rs.Week,
                    ReviewDate = rs.ReviewDate,
                    ReviewerName = rs.ReviewerName,
                    AcademicScore = rs.AcademicScore,
                    ReviewScoreValue = rs.ReviewScoreValue,
                    TaskScore = rs.TaskScore,
                    TotalScore = rs.TotalScore,
                    CreatedAt = rs.CreatedAt,
                    CreatedBy = rs.CreatedBy
                })
                .OrderByDescending(rs => rs.Week)
                .ToListAsync();
        }

        public async Task<ReviewScoreDto> CreateReviewScoreAsync(CreateReviewScoreDto dto, string createdBy)
        {
            // Check if review score already exists for this user and week
            var existingScore = await _context.ReviewScores
                .FirstOrDefaultAsync(rs => rs.UserId == dto.UserId && rs.Week == dto.Week);

            if (existingScore != null)
            {
                throw new InvalidOperationException($"Review score already exists for user in week {dto.Week}");
            }

            var reviewScore = new ReviewScore
            {
                UserId = dto.UserId,
                Week = dto.Week,
                ReviewDate = dto.ReviewDate,
                ReviewerName = dto.ReviewerName,
                AcademicScore = dto.AcademicScore,
                ReviewScoreValue = dto.ReviewScoreValue,
                TaskScore = dto.TaskScore,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ReviewScores.Add(reviewScore);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId);

            return new ReviewScoreDto
            {
                Id = reviewScore.Id,
                UserId = reviewScore.UserId,
                UserName = user?.FullName ?? "Unknown",
                Week = reviewScore.Week,
                ReviewDate = reviewScore.ReviewDate,
                ReviewerName = reviewScore.ReviewerName,
                AcademicScore = reviewScore.AcademicScore,
                ReviewScoreValue = reviewScore.ReviewScoreValue,
                TaskScore = reviewScore.TaskScore,
                TotalScore = reviewScore.TotalScore,
                CreatedAt = reviewScore.CreatedAt,
                CreatedBy = reviewScore.CreatedBy
            };
        }

        public async Task<ReviewScoreDto> UpdateReviewScoreAsync(int id, UpdateReviewScoreDto dto)
        {
            var reviewScore = await _context.ReviewScores
                .Include(rs => rs.User)
                .FirstOrDefaultAsync(rs => rs.Id == id);

            if (reviewScore == null) return null;

            reviewScore.Week = dto.Week;
            reviewScore.ReviewDate = dto.ReviewDate;
            reviewScore.ReviewerName = dto.ReviewerName;
            reviewScore.AcademicScore = dto.AcademicScore;
            reviewScore.ReviewScoreValue = dto.ReviewScoreValue;
            reviewScore.TaskScore = dto.TaskScore;
            reviewScore.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ReviewScoreDto
            {
                Id = reviewScore.Id,
                UserId = reviewScore.UserId,
                UserName = reviewScore.User.FullName,
                Week = reviewScore.Week,
                ReviewDate = reviewScore.ReviewDate,
                ReviewerName = reviewScore.ReviewerName,
                AcademicScore = reviewScore.AcademicScore,
                ReviewScoreValue = reviewScore.ReviewScoreValue,
                TaskScore = reviewScore.TaskScore,
                TotalScore = reviewScore.TotalScore,
                CreatedAt = reviewScore.CreatedAt,
                CreatedBy = reviewScore.CreatedBy
            };
        }

        public async Task<bool> DeleteReviewScoreAsync(int id)
        {
            var reviewScore = await _context.ReviewScores.FindAsync(id);
            if (reviewScore == null) return false;

            _context.ReviewScores.Remove(reviewScore);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanUserModifyReviewScoreAsync(int reviewScoreId, string userId, bool isAdmin)
        {
            if (isAdmin) return true;

            var reviewScore = await _context.ReviewScores
                .Include(rs => rs.User)
                .ThenInclude(u => u.Mentor)
                .FirstOrDefaultAsync(rs => rs.Id == reviewScoreId);

            if (reviewScore == null) return false;

            // Check if current user is the mentor of the user being reviewed
            var currentUserId = int.Parse(userId);
            return reviewScore.User.MentorId == currentUserId;
        }
    }
}