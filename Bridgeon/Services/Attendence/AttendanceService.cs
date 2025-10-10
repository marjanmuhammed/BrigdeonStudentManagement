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

        public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceCreateDto dto, string recordedById)
        {
            // Check duplicate
            var existing = await _repo.GetByUserDateAsync(dto.UserId, dto.Date);
            if (existing != null)
            {
                // if exists, update instead? Here throw
                throw new InvalidOperationException("Attendance already exists for this user and date.");
            }

            var entity = new Attendance
            {
                UserId = dto.UserId,
                Date = dto.Date.Date,
                Status = dto.Status,
                Notes = dto.Notes,
                RecordedById = recordedById,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<AttendanceDto> UpdateAttendanceAsync(int id, AttendanceUpdateDto dto, string modifiedById)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.Notes = dto.Notes;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.RecordedById = modifiedById;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<IEnumerable<AttendanceDto>> GetMonthAsync(int year, int month)
        {
            var items = await _repo.GetForMonthAsync(year, month);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetUserRangeAsync(string userId, DateTime from, DateTime to)
        {
            var items = await _repo.GetForUserRangeAsync(userId, from, to);
            return items.Select(MapToDto);
        }

        private AttendanceDto MapToDto(Attendance a)
        {
            return new AttendanceDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Date = a.Date,
                Status = a.Status,
                Notes = a.Notes,
                RecordedById = a.RecordedById
            };
        }
    }
}
