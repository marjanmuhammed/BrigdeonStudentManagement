using Bridgeon.Models;

namespace Bridgeon.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<Profile> GetByUserIdAsync(int userId);
        Task<Profile> CreateAsync(Profile profile);
        Task<Profile> UpdateAsync(Profile profile);
        Task<bool> DeleteAsync(int profileId);
        Task<bool> UserExistsAsync(int userId);
    }
}