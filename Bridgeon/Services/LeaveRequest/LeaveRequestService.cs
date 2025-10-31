using Bridgeon.Data;
using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.Models.Attendence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bridgeon.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly AppDbContext _context;

        public LeaveRequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequestDto> CreateLeaveRequestAsync(int userId, LeaveRequestCreateDto dto)
        {
            // Ensure we're only using the date part
            var dateOnly = dto.Date.Date;

            // Check if user already has a pending request for the same date
            var existingRequest = await _context.LeaveRequests
                .Where(lr => lr.UserId == userId &&
                           lr.Date.Date == dateOnly &&
                           lr.Status == LeaveRequestStatus.Pending)
                .FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                throw new InvalidOperationException("You already have a pending leave request for this date.");
            }

            var leaveRequest = new LeaveRequest
            {
                UserId = userId,
                Date = dateOnly,
                LeaveType = dto.LeaveType,
                Reason = dto.Reason,
                ProofImageUrl = dto.ProofImageUrl,
                Status = LeaveRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return await MapToDtoAsync(leaveRequest);
        }

        public async Task<LeaveRequestDto> GetByIdAsync(int id)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (request == null) return null;

            return await MapToDtoAsync(request);
        }

        public async Task<List<LeaveRequestDto>> GetUserRequestsAsync(int userId)
        {
            var requests = await _context.LeaveRequests
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.Date)
                .ToListAsync();

            var dtos = new List<LeaveRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

        public async Task<List<LeaveRequestDto>> GetPendingRequestsAsync(int reviewerId, string reviewerRole)
        {
            IQueryable<LeaveRequest> query = _context.LeaveRequests
                .Where(lr => lr.Status == LeaveRequestStatus.Pending);

            // If reviewer is Mentor, only show requests from their mentees
            if (reviewerRole == "Mentor")
            {
                // Get the list of mentee IDs for this mentor
                var menteeIds = await _context.UserMentors
                    .Where(um => um.MentorId == reviewerId)
                    .Select(um => um.MenteeId)
                    .ToListAsync();

                query = query.Where(lr => menteeIds.Contains(lr.UserId));
            }
            // If reviewer is Admin, show all pending requests (no filter)

            var requests = await query
                .OrderBy(lr => lr.Date)
                .ToListAsync();

            var dtos = new List<LeaveRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

        // ... rest of the existing methods ...
    

        public async Task<LeaveRequestDto> ReviewRequestAsync(LeaveRequestActionDto dto, int reviewerId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.Id == dto.RequestId);

            if (request == null)
                return null;

            if (request.Status != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("This request has already been processed.");
            }

            request.Status = dto.Approve ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
            request.ReviewedById = reviewerId;
            request.ReviewNotes = dto.Notes;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToDtoAsync(request);
        }

        private async Task<LeaveRequestDto> MapToDtoAsync(LeaveRequest request)
        {
            var user = await _context.Users
                .Where(u => u.Id == request.UserId)
                .Select(u => new { u.FullName, u.Email })
                .FirstOrDefaultAsync();

            return new LeaveRequestDto
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = user?.FullName ?? "Unknown User",
                UserEmail = user?.Email ?? "Unknown Email",
                Date = request.Date,
                LeaveType = request.LeaveType,
                Reason = request.Reason,
                ProofImageUrl = request.ProofImageUrl,
                Status = request.Status,
                ReviewedById = request.ReviewedById,
                ReviewNotes = request.ReviewNotes,
                CreatedAt = request.CreatedAt
            };
        }
        public async Task<List<LeaveRequestDto>> GetMenteesPendingRequestsAsync(int mentorId)
        {
            // Use the same approach as MentorService - check User.MentorId
            var menteeIds = await _context.Users
                .Where(u => u.MentorId == mentorId)
                .Select(u => u.Id)
                .ToListAsync();

            Console.WriteLine($"Mentor ID: {mentorId}");
            Console.WriteLine($"Found {menteeIds.Count} mentees for this mentor");

            if (!menteeIds.Any())
            {
                Console.WriteLine("No mentees found for this mentor");
                return new List<LeaveRequestDto>();
            }

            // Debug: Check if there are any pending leave requests for these mentees
            var pendingRequestsCount = await _context.LeaveRequests
                .Where(lr => lr.Status == LeaveRequestStatus.Pending &&
                             menteeIds.Contains(lr.UserId))
                .CountAsync();

            Console.WriteLine($"Found {pendingRequestsCount} pending leave requests for mentees");

            var requests = await _context.LeaveRequests
                .Where(lr => lr.Status == LeaveRequestStatus.Pending &&
                             menteeIds.Contains(lr.UserId))
                .OrderBy(lr => lr.Date)
                .ToListAsync();

            var dtos = new List<LeaveRequestDto>();
            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request));
            }

            return dtos;
        }

    }
}