using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Bridgeon.DTOs;
using Bridgeon.Services.Interfaces;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow admin/mentor to view any profile, users only their own
            if (currentUserRole == "User" && currentUserId != userId)
                return Forbid("Users can only view their own profile.");

            try
            {
                var profile = await _profileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                var profile = await _profileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> CreateProfile([FromBody] ProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var profile = await _profileService.CreateProfileAsync(dto);
                return CreatedAtAction(nameof(GetUserProfile), new { userId = dto.UserId }, profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] ProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var profile = await _profileService.UpdateProfileAsync(userId, dto);
                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{profileId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProfile(int profileId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                var result = await _profileService.DeleteProfileAsync(profileId, currentUserId, currentUserRole);
                if (result)
                    return NoContent();

                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}