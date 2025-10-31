using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bridgeon.Services.UserProfile;
using Bridgeon.Dtos.UserProfile;
using Bridgeon.Utils;
using System.Security.Claims;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not logged in");

            return int.Parse(userIdClaim);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentProfile()
        {
            try
            {
                var userId = GetUserId();
                var response = await _userProfileService.GetUserProfileAsync(userId);
                return StatusCode(response.Status, response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Unauthorized,
                    Message = "User not logged in"
                });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Invalid input data"
                });
            }

            var userId = GetUserId();
            var result = await _userProfileService.UpdateUserProfileAsync(userId, updateDto);

            return StatusCode(result.Status, result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Invalid input data"
                });
            }

            var userId = GetUserId();
            var result = await _userProfileService.ChangePasswordAsync(userId, changePasswordDto);

            return StatusCode(result.Status, result);
        }

        [HttpPut("profile-image")]
        public async Task<IActionResult> UpdateProfileImage([FromBody] UpdateProfileImageDto updateDto)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Invalid image URL"
                });
            }

            var userId = GetUserId();
            var result = await _userProfileService.UpdateProfileImageAsync(userId, updateDto.ImageUrl);

            return StatusCode(result.Status, result);
        }

        [HttpGet("all-profile-images")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> GetAllProfileImages()
        {
            try
            {
                var users = await _userProfileService.GetAllProfileImagesAsync();
                return StatusCode(users.Status, users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"Error fetching profile images: {ex.Message}"
                });
            }
        }

    }
}
