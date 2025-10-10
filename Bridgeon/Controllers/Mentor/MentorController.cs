using Bridgeon.Dtos.Mentor;
using Bridgeon.Services.Mentor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentorController : ControllerBase
    {
        private readonly IMentorService _mentorService;
        private readonly ILogger<MentorController> _logger;

        public MentorController(IMentorService mentorService, ILogger<MentorController> logger)
        {
            _mentorService = mentorService;
            _logger = logger;
        }

        // ✅ Admin: assign mentees to a mentor
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign([FromBody] MentorAssignDto dto)
        {
            try
            {
                await _mentorService.AssignMenteesAsync(dto);
                return Ok(new { message = "Assigned mentees to mentor successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning mentees");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ Admin: unassign mentees from mentor
        [HttpPost("unassign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unassign([FromBody] MentorUnassignDto dto)
        {
            try
            {
                await _mentorService.UnassignMenteesAsync(dto);
                return Ok(new { message = "Unassigned mentees successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning mentees");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ Mentor: get their own mentees
        [HttpGet("my-mentees")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> MyMentees()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var mentorId))
                    return Unauthorized(new { message = "Invalid token" });

                var mentees = await _mentorService.GetMenteesForMentorAsync(mentorId);
                return Ok(mentees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching mentor mentees");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ Admin or Mentor: get mentees for specific mentorId
        [HttpGet("{mentorId}/mentees")]
        [Authorize]
        public async Task<IActionResult> GetMenteesForMentor(int mentorId)
        {
            try
            {
                var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var requesterRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                if (requesterRole == "Mentor" &&
                    (!int.TryParse(requesterId, out var rid) || rid != mentorId))
                    return Forbid();

                var mentees = await _mentorService.GetMenteesForMentorAsync(mentorId);
                return Ok(mentees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mentees for mentor {MentorId}", mentorId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ Admin: get all mentors
        [HttpGet("admin/mentors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMentors()
        {
            try
            {
                var mentors = await _mentorService.GetAllMentorsAsync();
                return Ok(mentors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all mentors");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ Admin: get all students (for assignment)
        // ✅ Admin: Get all users with their mentor info
        [HttpGet("admin/users-with-mentor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersWithMentor()
        {
            try
            {
                var users = await _mentorService.GetAllUsersWithMentorAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with mentor info");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
