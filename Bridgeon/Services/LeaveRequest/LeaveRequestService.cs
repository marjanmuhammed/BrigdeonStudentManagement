using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.Models.Attendence;
using Bridgeon.Repositeries;
using Bridgeon.Repositeries.Attendence;
using Bridgeon.Services;
using Bridgeon.DTOs;
using Bridgeon.Models;
using Bridgeon.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repo;
        private readonly IAttendanceRepository _attendanceRepo;

        public LeaveRequestService(ILeaveRequestRepository repo, IAttendanceRepository attendanceRepo)
        {
            _repo = repo;
            _attendanceRepo = attendanceRepo;
        }

        public async Task<LeaveRequestDto> CreateLeaveRequestAsync(string userId, LeaveRequestCreateDto dto)
        {
            // Optional: check duplicates
            var existing = (await _repo.GetByUserAsync(userId))
                .FirstOrDefault(l => l.Date.Date == dto.Date.Date && l.Status == LeaveRequestStatus.Pending);

            if (existing != null)
                throw new InvalidOperationException("There is already a pending leave request for this date.");

            var leave = new LeaveRequest
            {
                UserId = userId,
                Date = dto.Date.Date,
                Reason = dto.Reason,
                Status = LeaveRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(leave);
            await _repo.SaveChangesAsync();

            return MapToDto(leave);
        }

        public async Task<LeaveRequestDto> GetByIdAsync(int id)
        {
            var leave = await _repo.GetByIdAsync(id);
            return leave == null ? null : MapToDto(leave);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync()
        {
            var list = await _repo.GetPendingAsync();
            return list.Select(MapToDto);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetUserRequestsAsync(string userId)
        {
            var list = await _repo.GetByUserAsync(userId);
            return list.Select(MapToDto);
        }

        public async Task<LeaveRequestDto> ReviewRequestAsync(LeaveRequestActionDto actionDto, string reviewedById)
        {
            var leave = await _repo.GetByIdAsync(actionDto.RequestId);
            if (leave == null) return null;
            if (leave.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Request already reviewed.");

            if (actionDto.Approve)
            {
                leave.Status = LeaveRequestStatus.Approved;
                // Optionally: create an Attendance record as Excused
                var attendance = await _attendanceRepo.GetByUserDateAsync(leave.UserId, leave.Date);
                if (attendance == null)
                {
                    var att = new Attendance
                    {
                        UserId = leave.UserId,
                        Date = leave.Date.Date,
                        Status = AttendanceStatus.Excused,
                        Notes = "Auto-marked from approved leave",
                        RecordedById = reviewedById,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _attendanceRepo.AddAsync(att);
                    // don't save attendance here; we save after updating leave
                }
            }
            else
            {
                leave.Status = LeaveRequestStatus.Rejected;
            }

            leave.ReviewedById = reviewedById;
            leave.ReviewNotes = actionDto.Notes;
            leave.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(leave);
            // persist both leave and attendance
            await _attendanceRepo.SaveChangesAsync();
            await _repo.SaveChangesAsync();

            return MapToDto(leave);
        }

        private LeaveRequestDto MapToDto(LeaveRequest lr)
        {
            return new LeaveRequestDto
            {
                Id = lr.Id,
                UserId = lr.UserId,
                Date = lr.Date,
                Reason = lr.Reason,
                Status = lr.Status,
                ReviewedById = lr.ReviewedById,
                ReviewNotes = lr.ReviewNotes
            };
        }
    }
}
