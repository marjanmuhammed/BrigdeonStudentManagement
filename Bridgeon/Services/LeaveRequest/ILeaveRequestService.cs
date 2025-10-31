using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestDto> CreateLeaveRequestAsync(int userId, LeaveRequestCreateDto dto);
        Task<LeaveRequestDto> GetByIdAsync(int id);
        Task<List<LeaveRequestDto>> GetUserRequestsAsync(int userId);
        Task<List<LeaveRequestDto>> GetPendingRequestsAsync(int reviewerId, string reviewerRole); // Updated
        Task<LeaveRequestDto> ReviewRequestAsync(LeaveRequestActionDto dto, int reviewerId);
        Task<List<LeaveRequestDto>> GetMenteesPendingRequestsAsync(int mentorId);
    }
}
