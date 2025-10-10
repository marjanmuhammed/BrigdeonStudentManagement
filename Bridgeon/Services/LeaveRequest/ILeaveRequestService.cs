using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestDto> CreateLeaveRequestAsync(string userId, LeaveRequestCreateDto dto);
        Task<IEnumerable<LeaveRequestDto>> GetUserRequestsAsync(string userId);
        Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync();
        Task<LeaveRequestDto> GetByIdAsync(int id);
        Task<LeaveRequestDto> ReviewRequestAsync(LeaveRequestActionDto actionDto, string reviewedById);
    }
}
