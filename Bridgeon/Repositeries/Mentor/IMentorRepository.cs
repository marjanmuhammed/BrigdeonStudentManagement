using Bridgeon.Models;

namespace Bridgeon.Repositeries.Mentor
{
    public interface IMentorRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetMenteesAsync(int mentorId);
        Task AssignMenteesAsync(int mentorId, IEnumerable<int> userIds);
        Task UnassignMenteesAsync(int mentorId, IEnumerable<int> userIds);
        Task SaveChangesAsync();
    }
}
