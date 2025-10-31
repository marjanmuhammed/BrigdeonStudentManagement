using Bridgeon.Dtos.Attendance;
using Bridgeon.Models.Attendence;
using Bridgeon.Repositeries.Attendence;
using Bridgeon.Services;
using Bridgeon.DTOs;
using Bridgeon.Models;
using Bridgeon.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        public AttendanceService(IAttendanceRepository repo) { _repo = repo; }

        public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceCreateDto dto, int recordedById)
        {
            var existing = await _repo.GetByUserDateAsync(dto.UserId, dto.Date);
            if (existing != null)
            {
                throw new InvalidOperationException("Attendance already exists for this user and date.");
            }

            var entity = new Attendance
            {
                UserId = dto.UserId,
                Date = dto.Date.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status,
                RecordedById = recordedById,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<AttendanceDto> UpdateAttendanceAsync(AttendanceUpdateDto dto, int modifiedById)
        {
            var entity = await _repo.GetByUserDateAsync(dto.UserId, dto.Date);
            if (entity == null)
                throw new InvalidOperationException("Attendance record not found for this user and date.");

            entity.Date = dto.Date.Date;
            entity.CheckInTime = dto.CheckInTime;
            entity.CheckOutTime = dto.CheckOutTime;
            entity.Status = dto.Status;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.RecordedById = modifiedById;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAttendanceAsync(int userId, DateTime date)
        {
            var entity = await _repo.GetByUserDateAsync(userId, date);
            if (entity == null) return false;

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AttendanceDto>> GetMonthAsync(int year, int month, int? userId = null)
        {
            var items = await _repo.GetForMonthAsync(year, month, userId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetUserAttendancesAsync(int userId)
        {
            var items = await _repo.GetUserAttendancesAsync(userId);
            return items.Select(MapToDto);
        }

        public async Task<AttendanceDto> GetByUserAndDateAsync(int userId, DateTime date)
        {
            var item = await _repo.GetByUserDateAsync(userId, date);
            return item == null ? null : MapToDto(item);
        }

        private AttendanceDto MapToDto(Attendance a)
        {
            return new AttendanceDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Status = a.Status,
                RecordedById = a.RecordedById,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt
            };
        }
    }
}
