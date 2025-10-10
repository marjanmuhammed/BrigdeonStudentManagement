using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridgeon.Models.Attendence;



namespace Bridgeon.Repositeries
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest> GetByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetByUserAsync(string userId);
        Task<IEnumerable<LeaveRequest>> GetPendingAsync();
        Task AddAsync(LeaveRequest leave);
        Task UpdateAsync(LeaveRequest leave);
        Task SaveChangesAsync();
    }
}
