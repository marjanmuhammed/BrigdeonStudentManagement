using Bridgeon.Dtos.ReviewStatusDto;
using Bridgeon.Models;
using Bridgeon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserReviewController : ControllerBase
    {
        private readonly IUserReviewService _service;

        public UserReviewController(IUserReviewService service)
        {
            _service = service;
        }

        // ======= 1️⃣ User: Get their own review =======
        [Authorize(Roles = "User")]
        [HttpGet("myreview")]
        public async Task<IActionResult> GetMyReview()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var userId = int.Parse(userIdClaim.Value);
            var review = await _service.GetUserReviewByUserIdAsync(userId);

            if (review == null)
                return NotFound("No review found for this user.");

            return Ok(MapToDto(review));
        }

        // ======= 2️⃣ Admin/Mentor: Create review =======
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewStatusDto dto)
        {
            if (dto == null || dto.UserId <= 0)
                return BadRequest("Invalid request data.");

            var review = new UserReview
            {
                UserId = dto.UserId,
                ReviewStatus = dto.ReviewStatus ?? "Not Assigned",
                ReviewDate = dto.ReviewDate ?? DateTime.UtcNow
            };

            var created = await _service.CreateUserReviewAsync(review);

            // ✅ Return Ok instead of CreatedAtAction (to avoid invalid route)
            return Ok(MapToDto(created));
        }

        // ======= 3️⃣ Admin/Mentor: Update review =======
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewStatusDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid review data.");

            var updated = await _service.UpdateReviewStatusAsync(id, dto.ReviewStatus, dto.ReviewDate);
            if (updated == null)
                return NotFound();

            return Ok(MapToDto(updated));
        }

        // ======= 4️⃣ Admin/Mentor: Delete review =======
        [Authorize(Roles = "Admin,Mentor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var deleted = await _service.DeleteUserReviewAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        // ======= 5️⃣ Admin/Mentor: Add or Update pending fee =======
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost("{userId}/pendingfee")]
        public async Task<IActionResult> AddOrUpdatePendingFee(int userId, [FromBody] PendingFeesDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid fee data.");

            var review = await _service.GetUserReviewByUserIdAsync(userId);

            if (review == null)
            {
                // Create new review record with pending fee
                review = new UserReview
                {
                    UserId = userId,
                    FeeCategory = dto.FeeCategory,
                    PendingAmount = dto.PendingAmount,
                    DueDate = dto.DueDate,
                    FeeStatus = dto.FeeStatus,
                    ReviewDate = DateTime.UtcNow
                };

                var created = await _service.CreateUserReviewAsync(review);
                return Ok(MapToDto(created));
            }
            else
            {
                var updated = await _service.UpdatePendingFeeAsync(
                    review.Id,
                    dto.FeeCategory,
                    dto.PendingAmount,
                    dto.DueDate
                );

                return Ok(MapToDto(updated));
            }
        }

        // ======= 6️⃣ Admin/Mentor: Get review by Id (optional route) =======
        [Authorize(Roles = "Admin,Mentor,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _service.GetUserReviewAsync(id);
            if (review == null)
                return NotFound();

            return Ok(MapToDto(review));
        }

        // ======= Helper: Mapper =======
        private UserReviewDto MapToDto(UserReview review) => new()
        {
            Id = review.Id,
            UserId = review.UserId,
            ReviewStatus = review.ReviewStatus,
            ReviewDate = review.ReviewDate,
            FeeCategory = review.FeeCategory,
            PendingAmount = review.PendingAmount,
            DueDate = review.DueDate,
            FeeStatus = review.FeeStatus,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            UserFullName = review.User?.FullName,
            UserEmail = review.User?.Email
        };
    }
}
