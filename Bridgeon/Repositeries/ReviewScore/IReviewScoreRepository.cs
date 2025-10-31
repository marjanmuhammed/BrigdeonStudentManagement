using Bridgeon.Dtos;
using Bridgeon.Models;


namespace Bridgeon.Repositories
{
    public interface IReviewScoreRepository
    {
        Task<IEnumerable<ReviewScoreDto>> GetReviewScoresAsync();
        Task<ReviewScoreDto> GetReviewScoreByIdAsync(int id);
        Task<IEnumerable<ReviewScoreDto>> GetReviewScoresByUserIdAsync(int userId);
        Task<ReviewScoreDto> CreateReviewScoreAsync(CreateReviewScoreDto dto, string createdBy);
        Task<ReviewScoreDto> UpdateReviewScoreAsync(int id, UpdateReviewScoreDto dto);
        Task<bool> DeleteReviewScoreAsync(int id);
        Task<bool> CanUserModifyReviewScoreAsync(int reviewScoreId, string userId, bool isAdmin);
    }
}