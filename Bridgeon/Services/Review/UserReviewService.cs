using Bridgeon.Data;
using Bridgeon.Dtos.ReviewStatusDto;
using Bridgeon.Models;
using Bridgeon.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public class UserReviewService : IUserReviewService
    {
        private readonly AppDbContext _context;
        public UserReviewService(AppDbContext context) => _context = context;

        // ======= Get all reviews by userId =======
        public async Task<IEnumerable<UserReview>> GetUserReviewsByUserIdAsync(int userId)
        {
            return await _context.UserReviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // ======= Get single review by review Id =======
        public async Task<UserReview> GetUserReviewAsync(int id)
        {
            return await _context.UserReviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ======= Get single review by userId (latest if multiple) =======
        public async Task<UserReview> GetUserReviewByUserIdAsync(int userId)
        {
            return await _context.UserReviews
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // ======= Create review =======
        public async Task<UserReview> CreateUserReviewAsync(UserReview review)
        {
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            _context.UserReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        // ======= Update review status & date =======
        public async Task<UserReview> UpdateReviewStatusAsync(int id, string status, DateTime? reviewDate)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.ReviewStatus = status;
            review.ReviewDate = reviewDate;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return review;
        }

        // ======= Update pending fees =======
        public async Task<UserReview> UpdatePendingFeeAsync(int id, string feeCategory, decimal pendingAmount, DateTime? dueDate)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.FeeCategory = feeCategory;
            review.PendingAmount = pendingAmount;
            review.DueDate = dueDate;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return review;
        }

        // ======= Delete review =======
        public async Task<bool> DeleteUserReviewAsync(int id)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return false;

            _context.UserReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        // ======= Map UserReview → UserReviewDto =======
        public UserReviewDto MapToDto(UserReview r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            ReviewStatus = r.ReviewStatus,
            ReviewDate = r.ReviewDate,
            FeeCategory = r.FeeCategory,
            PendingAmount = r.PendingAmount,
            DueDate = r.DueDate,
            FeeStatus = r.FeeStatus,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            UserFullName = r.User?.FullName,
            UserEmail = r.User?.Email
        };
    }
}
