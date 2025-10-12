using Bridgeon.Dtos;
using Bridgeon.Dtos.CreateReview;
using Bridgeon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services.Interfaces
{
    public interface IUserReviewService
    {
        Task<IEnumerable<UserReview>> GetAllUserReviewsAsync();
        Task<UserReview> GetUserReviewAsync(int id);
        Task<UserReview> GetUserReviewByUserIdAsync(int userId);
        Task<IEnumerable<UserReview>> GetUserReviewsByUserIdAsync(int userId);
        Task<UserReview> CreateUserReviewAsync(UserReview review);
        Task<UserReview> UpdateReviewStatusAsync(int id, string status, DateTime? reviewDate);
        Task<UserReview> UpdatePendingFeeByReviewIdAsync(int id, string feeCategory, decimal pendingAmount, DateTime? dueDate);
        Task<UserReview> UpdateFeeStatusAsync(int id, string feeStatus);
        Task<bool> DeleteUserReviewAsync(int id);
        Task<UserReview> DeletePendingFeeByReviewIdAsync(int id);
        UserReviewDto MapToDto(UserReview review);
    }
}