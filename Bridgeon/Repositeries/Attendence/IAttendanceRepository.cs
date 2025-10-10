using Bridgeon.Models.Attendence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Repositeries.Attendence
{
    public interface IAttendanceRepository
    {
        Task<Attendance> GetByIdAsync(int id);
        Task<Attendance> GetByUserDateAsync(string userId, DateTime date);
        Task<IEnumerable<Attendance>> GetForUserRangeAsync(string userId, DateTime from, DateTime to);
        Task<IEnumerable<Attendance>> GetForMonthAsync(int year, int month);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(Attendance attendance);
        Task SaveChangesAsync();
    }
}
