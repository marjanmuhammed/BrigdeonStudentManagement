using Bridgeon.Data;
using Bridgeon.Dtos;
using Bridgeon.Models;
using Bridgeon.Repositories;
using Bridgeon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewScoresController : ControllerBase
    {
        private readonly IReviewScoreRepository _reviewScoreRepository;
        private readonly AppDbContext _context; // ✅ Declare AppDbContext
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReviewScoresController> _logger;

        public ReviewScoresController(IReviewScoreRepository reviewScoreRepository, AppDbContext context, INotificationService notificationService,
            ILogger<ReviewScoresController> logger)
        {
            _reviewScoreRepository = reviewScoreRepository;
            _context = context; // ✅ Assign context
            _notificationService = notificationService;
            _logger = logger;
        }

        // ✅ Get all review scores (Admin, Mentor)
        [HttpGet]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<ActionResult<IEnumerable<ReviewScoreDto>>> GetReviewScores()
        {
            var reviewScores = await _reviewScoreRepository.GetReviewScoresAsync();
            return Ok(reviewScores);
        }

        // ✅ Get single review score by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewScoreDto>> GetReviewScore(int id)
        {
            var reviewScore = await _reviewScoreRepository.GetReviewScoreByIdAsync(id);
            if (reviewScore == null) return NotFound();
            return Ok(reviewScore);
        }

        // ✅ Get review scores by specific user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewScoreDto>>> GetReviewScoresByUser(int userId)
        {
            var reviewScores = await _reviewScoreRepository.GetReviewScoresByUserIdAsync(userId);
            return Ok(reviewScores);
        }

        // ✅ Create a new review score
        [HttpPost]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<ActionResult<ReviewScoreDto>> CreateReviewScore(CreateReviewScoreDto dto)
        {
            try
            {
                var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var reviewScore = await _reviewScoreRepository.CreateReviewScoreAsync(dto, createdBy);

                // ✅ Create notification for review score creation
                await _notificationService.CreateNotificationAsync(new Notification
                {
                    UserId = dto.UserId,
                    Title = "Review Score Added",
                    Message = $"New review score has been added for your review",
                    Type = "review_score",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetReviewScore), new { id = reviewScore.Id }, reviewScore);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<ActionResult<ReviewScoreDto>> UpdateReviewScore(int id, UpdateReviewScoreDto dto)
        {
            try
            {
                // Get the existing review score to know the user ID
                var existingScore = await _reviewScoreRepository.GetReviewScoreByIdAsync(id);
                if (existingScore == null) return NotFound();

                var updatedReviewScore = await _reviewScoreRepository.UpdateReviewScoreAsync(id, dto);
                if (updatedReviewScore == null) return NotFound();

                // ✅ Create notification for review score update - "Review Score Updated"
                await _notificationService.CreateNotificationAsync(new Notification
                {
                    UserId = existingScore.UserId,
                    Title = "Review Score Updated",
                    Message = $"Your review score has been updated",
                    Type = "review_score",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                return Ok(updatedReviewScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review score");
                return StatusCode(500, new { message = "An error occurred while updating review score" });
            }
        }

        // ✅ Delete a review score
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> DeleteReviewScore(int id)
        {
            var result = await _reviewScoreRepository.DeleteReviewScoreAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // ✅ Get currently logged-in user info
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var user = await _context.Users
                .Include(u => u.Mentor)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                MentorName = user.Mentor?.FullName
            });
        }
    }
}
