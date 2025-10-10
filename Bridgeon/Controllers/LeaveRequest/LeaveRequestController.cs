using Bridgeon.Dtos.LeaveRequest;
using Bridgeon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgeon.DTOs;
using Bridgeon.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _service;
        public LeaveRequestController(ILeaveRequestService service) { _service = service; }

        // Create leave request (authenticated user)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] LeaveRequestCreateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var res = await _service.CreateLeaveRequestAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        // Get requests for current user
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var res = await _service.GetUserRequestsAsync(userId);
            return Ok(res);
        }

        // Admin/Mentor: list pending requests
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Pending()
        {
            var res = await _service.GetPendingRequestsAsync();
            return Ok(res);
        }

        // Admin/Mentor: approve/reject
        [HttpPost("review")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Review([FromBody] LeaveRequestActionDto dto)
        {
            var reviewer = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            try
            {
                var res = await _service.ReviewRequestAsync(dto, reviewer);
                if (res == null) return NotFound();
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
