using Bridgeon.Models;

namespace Bridgeon.Repositories
{
    public interface IUserProfileRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> VerifyPasswordAsync(int userId, string password);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);

        Task<IEnumerable<User>> GetAllUsersAsync();

    }
}