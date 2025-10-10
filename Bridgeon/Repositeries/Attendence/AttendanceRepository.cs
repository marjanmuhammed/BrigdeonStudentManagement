using Bridgeon.Data;
using Bridgeon.Models.Attendence;
using Microsoft.EntityFrameworkCore;

namespace Bridgeon.Repositeries.Attendence
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _db;
        public AttendanceRepository(AppDbContext db) { _db = db; }

        public async Task AddAsync(Attendance attendance)
        {
            await _db.Attendances.AddAsync(attendance);
        }

        public async Task DeleteAsync(Attendance attendance)
        {
            _db.Attendances.Remove(attendance);
            await Task.CompletedTask;
        }

        public async Task<Attendance> GetByIdAsync(int id)
        {
            return await _db.Attendances.FindAsync(id);
        }

        public async Task<Attendance> GetByUserDateAsync(string userId, DateTime date)
        {
            var d = date.Date;
            return await _db.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date.Date == d);
        }

        public async Task<IEnumerable<Attendance>> GetForUserRangeAsync(string userId, DateTime from, DateTime to)
        {
            return await _db.Attendances
                .Where(a => a.UserId == userId && a.Date.Date >= from.Date && a.Date.Date <= to.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetForMonthAsync(int year, int month)
        {
            return await _db.Attendances
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .ToListAsync();
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            _db.Attendances.Update(attendance);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
