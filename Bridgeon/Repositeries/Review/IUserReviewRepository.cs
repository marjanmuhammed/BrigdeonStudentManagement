using Bridgeon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Repositories
{
    public interface IUserReviewRepository
    {
        Task<UserReview> GetByIdAsync(int id);
        Task<List<UserReview>> GetAllAsync();
        Task<List<UserReview>> GetByUserIdAsync(int userId);
        Task<UserReview> AddAsync(UserReview userReview);
        Task<UserReview> UpdateAsync(UserReview userReview);
        Task<bool> DeleteAsync(int id);
    }
}
