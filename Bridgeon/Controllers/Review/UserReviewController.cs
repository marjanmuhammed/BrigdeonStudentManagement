using Bridgeon.Dtos;
using Bridgeon.Dtos.CreateReview;
using Bridgeon.Models;
using Bridgeon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        // 1️⃣ Add review (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            if (dto == null || dto.UserId <= 0)
                return BadRequest(new { message = "Invalid request data." });

            var review = new UserReview
            {
                UserId = dto.UserId,
                ReviewStatus = string.IsNullOrEmpty(dto.ReviewStatus) ? "Not Assigned" : dto.ReviewStatus,
                ReviewDate = string.IsNullOrEmpty(dto.ReviewDate)
        ? DateTime.UtcNow
        : DateTime.Parse(dto.ReviewDate), // ⚡ parse from string
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _service.CreateUserReviewAsync(review);
            return Ok(new { message = "Review added successfully.", data = _service.MapToDto(created) });
        }


        // 2️⃣ Update review by userId (Admin/Mentor) - only status
        [HttpPut("update-review-status/{userId}")]
        public async Task<IActionResult> UpdateReviewStatus(int userId, [FromBody] UpdateStatusDto dto)
        {
            if (string.IsNullOrEmpty(dto.ReviewStatus))
                return BadRequest(new { message = "Review status is required." });

            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            var updated = await _service.UpdateReviewStatusAsync(review.Id, dto.ReviewStatus, dto.ReviewDate); // ✅ use dto.ReviewDate
            return Ok(new { message = "Review status updated successfully.", data = _service.MapToDto(updated) });
        }


        // 3️⃣ Delete review by userId (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpDelete("delete-review/{userId}")]
        public async Task<IActionResult> DeleteReview(int userId)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            var result = await _service.DeleteUserReviewAsync(review.Id);
            if (!result) return NotFound(new { message = "Review deletion failed." });

            return Ok(new { message = "Review deleted successfully." });
        }

        // 4️⃣ Get all reviews (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _service.GetAllUserReviewsAsync();
            var reviewDtos = new List<UserReviewDto>();
            foreach (var r in reviews) reviewDtos.Add(_service.MapToDto(r));

            return Ok(new { message = "All reviews fetched successfully.", data = reviewDtos });
        }

        // 5️⃣ Get review by userId (Admin/Mentor/User)
        [Authorize(Roles = "Admin,Mentor,User")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetReviewByUserId(int userId)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            return Ok(new { message = "Review fetched successfully.", data = _service.MapToDto(review) });
        }

        // 6️⃣ Get own reviews (User)
        [Authorize(Roles = "User")]
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid user token." });

            var reviews = await _service.GetUserReviewsByUserIdAsync(userId);
            var reviewDtos = new List<UserReviewDto>();
            foreach (var r in reviews) reviewDtos.Add(_service.MapToDto(r));

            return Ok(new { message = "Your reviews fetched successfully.", data = reviewDtos });
        }

        // 7️⃣ Add or update fees (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPost("{userId}/fees")]
        public async Task<IActionResult> AddOrUpdateFees(int userId, [FromBody] PendingFeesDto dto)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            var updated = await _service.UpdatePendingFeeByReviewIdAsync(review.Id, dto.FeeCategory, dto.PendingAmount, dto.DueDate);
            return Ok(new { message = "Fees updated successfully.", data = _service.MapToDto(updated) });
        }

        // 8️⃣ Update fee status (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpPut("{userId}/fee-status")]
        public async Task<IActionResult> UpdateFeeStatus(int userId, [FromBody] FeeStatusDto dto)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            var updated = await _service.UpdateFeeStatusAsync(review.Id, dto.FeeStatus);
            return Ok(new { message = "Fee status updated successfully.", data = _service.MapToDto(updated) });
        }

        // 9️⃣ Delete fee (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpDelete("{userId}/fees")]
        public async Task<IActionResult> DeleteFees(int userId)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            var updated = await _service.DeletePendingFeeByReviewIdAsync(review.Id);
            return Ok(new { message = "Fees deleted successfully.", data = _service.MapToDto(updated) });
        }

        // 🔟 Get fees for own user (User)
        [Authorize(Roles = "User")]
        [HttpGet("my-fees")]
        public async Task<IActionResult> GetMyFees()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid user token." });

            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "No review found for your account." });

            return Ok(new
            {
                message = "Your fees fetched successfully.",
                data = new
                {
                    review.FeeCategory,
                    review.PendingAmount,
                    review.DueDate,
                    review.FeeStatus
                }
            });
        }

        // 1️⃣1️⃣ Get fees by any userId (Admin/Mentor)
        [Authorize(Roles = "Admin,Mentor")]
        [HttpGet("{userId}/fees")]
        public async Task<IActionResult> GetFeesByUserId(int userId)
        {
            var review = await _service.GetUserReviewByUserIdAsync(userId);
            if (review == null) return NotFound(new { message = "Review not found." });

            return Ok(new
            {
                message = "User fees fetched successfully.",
                data = new
                {
                    review.FeeCategory,
                    review.PendingAmount,
                    review.DueDate,
                    review.FeeStatus
                }
            });
        }
    }
}
