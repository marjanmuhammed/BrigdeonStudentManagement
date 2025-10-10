using Bridgeon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services.Interfaces
{
    public interface IUserReviewService
    {
        Task<IEnumerable<UserReview>> GetUserReviewsByUserIdAsync(int userId);
        Task<UserReview> GetUserReviewAsync(int id);
        Task<UserReview> CreateUserReviewAsync(UserReview review);
        Task<UserReview> UpdateReviewStatusAsync(int id, string status, DateTime? reviewDate);
        Task<UserReview> UpdatePendingFeeAsync(int id, string feeCategory, decimal pendingAmount, DateTime? dueDate);
        Task<bool> DeleteUserReviewAsync(int id);

        // ✅ Add this method to support AddPendingFee
        Task<UserReview> GetUserReviewByUserIdAsync(int userId);
    }
}
