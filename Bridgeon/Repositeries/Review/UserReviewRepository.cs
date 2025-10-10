using Bridgeon.Models;
using Bridgeon.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Bridgeon.Data;

namespace Bridgeon.Repositories
{
    public class UserReviewRepository : IUserReviewRepository
    {
        private readonly AppDbContext _context;
        public UserReviewRepository(AppDbContext context) => _context = context;

        public async Task<UserReview> GetByIdAsync(int id) =>
            await _context.UserReviews.Include(ur => ur.User).FirstOrDefaultAsync(ur => ur.Id == id);

        public async Task<List<UserReview>> GetAllAsync() =>
            await _context.UserReviews.Include(ur => ur.User).ToListAsync();

        public async Task<List<UserReview>> GetByUserIdAsync(int userId) =>
            await _context.UserReviews.Where(ur => ur.UserId == userId).Include(ur => ur.User).ToListAsync();

        public async Task<UserReview> AddAsync(UserReview userReview)
        {
            userReview.CreatedAt = DateTime.UtcNow;
            userReview.UpdatedAt = DateTime.UtcNow;
            _context.UserReviews.Add(userReview);
            await _context.SaveChangesAsync();
            return userReview;
        }

        public async Task<UserReview> UpdateAsync(UserReview userReview)
        {
            userReview.UpdatedAt = DateTime.UtcNow;
            _context.UserReviews.Update(userReview);
            await _context.SaveChangesAsync();
            return userReview;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.UserReviews.FindAsync(id);
            if (review == null) return false;
            _context.UserReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
