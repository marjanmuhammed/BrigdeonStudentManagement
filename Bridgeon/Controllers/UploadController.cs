// Controllers/UploadController.cs
using Bridgeon.Services;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgeon.Data;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly AppDbContext _dbContext;

        public UploadController(ICloudinaryService cloudinaryService, AppDbContext dbContext)
        {
            _cloudinaryService = cloudinaryService;
            _dbContext = dbContext;
        }
        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = ApiStatusCodes.BadRequest,
                        Message = "No file uploaded"
                    });
                }

                // Upload image to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file);

                // Get current user ID from JWT claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized(new ApiResponse<object>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "User not authenticated"
                    });

                var userId = int.Parse(userIdClaim);

                // Update DB
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Status = ApiStatusCodes.NotFound,
                        Message = "User not found"
                    });

                user.ProfileImageUrl = imageUrl;
                await _dbContext.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Image uploaded and profile updated successfully",
                    Data = imageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(ApiStatusCodes.ServerError, new ApiResponse<object>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"Error uploading image: {ex.Message}"
                });
            }
        }

    }
}