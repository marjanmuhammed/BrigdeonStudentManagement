using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestController(ILeaveRequestService service)
        {
            _service = service;
        }

        // 🟢 Create leave request (Authenticated user)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] LeaveRequestCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            try
            {
                var res = await _service.CreateLeaveRequestAsync(userId, dto);
                return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 🟢 Get request by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            if (res == null)
                return NotFound();

            return Ok(res);
        }

        // 🟢 Get all requests for current user
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> MyRequests()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var res = await _service.GetUserRequestsAsync(userId);
            return Ok(res);
        }

        // 🟡 Admin or Mentor: list pending requests (updated)
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Pending()
        {
            var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (reviewerId == 0)
                return Unauthorized();

            // Get the role from JWT claims
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return Unauthorized();

            var res = await _service.GetPendingRequestsAsync(reviewerId, role);
            return Ok(res);
        }

        // 🔵 Admin or Mentor: approve/reject request
        [HttpPost("review")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Review([FromBody] LeaveRequestActionDto dto)
        {
            var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (reviewerId == 0)
                return Unauthorized();

            try
            {
                var res = await _service.ReviewRequestAsync(dto, reviewerId);
                if (res == null)
                    return NotFound();

                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 🟢 Mentor: Get pending leave requests for their own mentees
        [HttpGet("mentor/pending")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetMyMenteesPendingRequests()
        {
            try
            {
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (mentorId == 0)
                    return Unauthorized();

                var res = await _service.GetMenteesPendingRequestsAsync(mentorId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
