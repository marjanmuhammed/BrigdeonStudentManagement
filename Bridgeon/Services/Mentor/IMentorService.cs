using Bridgeon.Dtos.Mentor;
using Bridgeon.Models;

namespace Bridgeon.Services.Mentor
{
    public interface IMentorService
    {
        Task AssignMenteesAsync(MentorAssignDto dto);
        Task UnassignMenteesAsync(MentorUnassignDto dto);
        Task<IEnumerable<MentorMenteeDto>> GetMenteesForMentorAsync(int mentorId);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllMentorsAsync();
       

        Task<IEnumerable<MentorMenteeDto>> GetAllUsersWithMentorAsync();

    }
}
