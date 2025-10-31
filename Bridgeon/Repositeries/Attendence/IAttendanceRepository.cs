using Bridgeon.Models.Attendence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Repositeries.Attendence
{
    public interface IAttendanceRepository
    {
        Task<Attendance> GetByIdAsync(int id);
        Task<Attendance> GetByUserDateAsync(int userId, DateTime date);
        Task<IEnumerable<Attendance>> GetForUserRangeAsync(int userId, DateTime from, DateTime to);
        Task<IEnumerable<Attendance>> GetForMonthAsync(int year, int month, int? userId = null);
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<IEnumerable<Attendance>> GetUserAttendancesAsync(int userId);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(Attendance attendance);
        Task SaveChangesAsync();
    }
}
