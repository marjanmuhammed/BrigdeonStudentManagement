using Bridgeon.Dtos.Attendance;
using Bridgeon.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> CreateAttendanceAsync(AttendanceCreateDto dto, int recordedById);
        Task<AttendanceDto> UpdateAttendanceAsync(AttendanceUpdateDto dto, int modifiedById);
        Task<bool> DeleteAttendanceAsync(int userId, DateTime date);
        Task<IEnumerable<AttendanceDto>> GetMonthAsync(int year, int month, int? userId = null);
        Task<IEnumerable<AttendanceDto>> GetAllAsync();
        Task<IEnumerable<AttendanceDto>> GetUserAttendancesAsync(int userId);
        Task<AttendanceDto> GetByUserAndDateAsync(int userId, DateTime date);
    }
}
