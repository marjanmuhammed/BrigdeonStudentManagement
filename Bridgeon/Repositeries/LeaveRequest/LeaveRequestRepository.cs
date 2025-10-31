using Bridgeon.Data;
using Bridgeon.Models.Attendence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Bridgeon.Repositeries
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly AppDbContext _db;
        public LeaveRequestRepository(AppDbContext db) { _db = db; }

        public async Task AddAsync(LeaveRequest leave)
        {
            await _db.LeaveRequests.AddAsync(leave);
        }

        public async Task<LeaveRequest> GetByIdAsync(int id)
        {
            return await _db.LeaveRequests.FindAsync(id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByUserAsync(int userId)
        {
            return await _db.LeaveRequests
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
        {
            return await _db.LeaveRequests
                .Where(l => l.Status == LeaveRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task UpdateAsync(LeaveRequest leave)
        {
            _db.LeaveRequests.Update(leave);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
