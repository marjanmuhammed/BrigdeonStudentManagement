using Bridgeon.Models;

namespace Bridgeon.Repositeries.UserRepo
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();

        Task<IEnumerable<User>> GetBlockedUsersAsync(); // ✅ new
        void Update(User user);
        void Remove(User user);
        Task SaveChangesAsync();
    }
}
