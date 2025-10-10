using Bridgeon.Dtos.Attendance;
using Bridgeon.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> CreateAttendanceAsync(AttendanceCreateDto dto, string recordedById);
        Task<AttendanceDto> UpdateAttendanceAsync(int id, AttendanceUpdateDto dto, string modifiedById);
        Task<bool> DeleteAttendanceAsync(int id);
        Task<IEnumerable<AttendanceDto>> GetMonthAsync(int year, int month);
        Task<IEnumerable<AttendanceDto>> GetUserRangeAsync(string userId, DateTime from, DateTime to);
    }
}
