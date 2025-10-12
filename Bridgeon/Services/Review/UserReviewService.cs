using Bridgeon.Data;
using Bridgeon.Dtos;
using Bridgeon.Dtos.CreateReview;
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

        public UserReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserReview>> GetAllUserReviewsAsync()
        {
            return await _context.UserReviews.Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<UserReview> GetUserReviewAsync(int id)
        {
            return await _context.UserReviews.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<UserReview>> GetUserReviewsByUserIdAsync(int userId)
        {
            return await _context.UserReviews.Where(r => r.UserId == userId).Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<UserReview> CreateUserReviewAsync(UserReview review)
        {
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            _context.UserReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<UserReview> GetUserReviewByUserIdAsync(int userId)
        {
            // Fetch the latest review for the user
            return await _context.UserReviews
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }


        public async Task<UserReview> UpdateReviewStatusAsync(int id, string status, DateTime? reviewDate = null)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.ReviewStatus = status;

            // ✅ Only update reviewDate if a value is provided
            if (reviewDate.HasValue)
                review.ReviewDate = reviewDate.Value;

            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetUserReviewAsync(review.Id);
        }

        public async Task<UserReview> UpdatePendingFeeByReviewIdAsync(int id, string feeCategory, decimal pendingAmount, DateTime? dueDate)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.FeeCategory = feeCategory;
            review.PendingAmount = pendingAmount;
            review.DueDate = dueDate;
            review.FeeStatus = "Pending";
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetUserReviewAsync(review.Id);
        }

        public async Task<UserReview> UpdateFeeStatusAsync(int id, string feeStatus)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.FeeStatus = feeStatus;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetUserReviewAsync(review.Id);
        }

        public async Task<UserReview> DeletePendingFeeByReviewIdAsync(int id)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return null;

            review.FeeCategory = null;
            review.PendingAmount = null;
            review.DueDate = null;
            review.FeeStatus = null;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetUserReviewAsync(review.Id);
        }

        public async Task<bool> DeleteUserReviewAsync(int id)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return false;

            _context.UserReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public UserReviewDto MapToDto(UserReview r) => new UserReviewDto
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
//////////////////////////////////////