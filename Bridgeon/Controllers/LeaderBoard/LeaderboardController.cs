// Controllers/LeaderboardController.cs
using Bridgeon.Data;
using Bridgeon.Dtos.Leaderboard;
using Bridgeon.Models;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(AppDbContext context, ILogger<LeaderboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Get leaderboard with user details and profile images
        [HttpGet]
        [Authorize(Roles = "Admin,Mentor,User")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaderboardDto>>>> GetLeaderboard()
        {
            try
            {
                _logger.LogInformation("Fetching leaderboard data");

                // Get all review scores with user details
                var allScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.User.FullName,
                        rs.User.ProfileImageUrl,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                // Calculate TotalScore in memory and filter
                var leaderboardData = allScores
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.FullName,
                        rs.ProfileImageUrl,
                        TotalScore = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .Where(rs => rs.TotalScore > 0)
                    .GroupBy(rs => rs.UserId)
                    .Select(g => new LeaderboardDto
                    {
                        UserId = g.Key,
                        FullName = g.First().FullName ?? "Unknown User",
                        ProfileImageUrl = g.First().ProfileImageUrl,
                        TotalScore = g.Max(rs => rs.TotalScore),
                        LatestReviewDate = g.Max(rs => rs.ReviewDate)
                    })
                    .OrderByDescending(u => u.TotalScore)
                    .ThenByDescending(u => u.LatestReviewDate)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < leaderboardData.Count; i++)
                {
                    leaderboardData[i].Rank = i + 1;
                }

                _logger.LogInformation($"Successfully processed {leaderboardData.Count} users for leaderboard");

                return Ok(new ApiResponse<IEnumerable<LeaderboardDto>>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Leaderboard data fetched successfully",
                    Data = leaderboardData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard data: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"An error occurred while fetching leaderboard data: {ex.Message}",
                    Data = null
                });
            }
        }

        // ✅ Get top N users for leaderboard (for dashboard display)
        [HttpGet("top/{count}")]
        [Authorize(Roles = "Admin,Mentor,User")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaderboardDto>>>> GetTopUsers(int count = 3)
        {
            try
            {
                if (count <= 0 || count > 100)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.BadRequest,
                        Message = "Count must be between 1 and 100"
                    });
                }

                _logger.LogInformation($"Fetching top {count} users for leaderboard");

                // Get all scores first
                var allScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.User.FullName,
                        rs.User.ProfileImageUrl,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                // Calculate TotalScore in memory
                var topUsers = allScores
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.FullName,
                        rs.ProfileImageUrl,
                        TotalScore = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .Where(rs => rs.TotalScore > 0)
                    .GroupBy(rs => rs.UserId)
                    .Select(g => new LeaderboardDto
                    {
                        UserId = g.Key,
                        FullName = g.First().FullName ?? "Unknown User",
                        ProfileImageUrl = g.First().ProfileImageUrl,
                        TotalScore = g.Max(rs => rs.TotalScore),
                        LatestReviewDate = g.Max(rs => rs.ReviewDate)
                    })
                    .OrderByDescending(u => u.TotalScore)
                    .ThenByDescending(u => u.LatestReviewDate)
                    .Take(count)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < topUsers.Count; i++)
                {
                    topUsers[i].Rank = i + 1;
                }

                _logger.LogInformation($"Returning top {topUsers.Count} users");

                return Ok(new ApiResponse<IEnumerable<LeaderboardDto>>
                {
                    Status = ApiStatusCodes.Success,
                    Message = $"Top {count} users fetched successfully",
                    Data = topUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching top {count} users: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"An error occurred while fetching top users: {ex.Message}",
                    Data = null
                });
            }
        }

        // ✅ Get current user's position in leaderboard
        [HttpGet("my-position")]
        [Authorize(Roles = "Admin,Mentor,User")]
        public async Task<ActionResult<ApiResponse<LeaderboardDto>>> GetMyLeaderboardPosition()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "User ID not found in token"
                    });
                }

                var currentUserId = int.Parse(userId);
                _logger.LogInformation($"Fetching leaderboard position for user {currentUserId}");

                // Get all scores first
                var allScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.User.FullName,
                        rs.User.ProfileImageUrl,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                // Calculate TotalScore in memory
                var userScores = allScores
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.FullName,
                        rs.ProfileImageUrl,
                        TotalScore = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .Where(rs => rs.TotalScore > 0)
                    .GroupBy(rs => rs.UserId)
                    .Select(g => new LeaderboardDto
                    {
                        UserId = g.Key,
                        FullName = g.First().FullName ?? "Unknown User",
                        ProfileImageUrl = g.First().ProfileImageUrl,
                        TotalScore = g.Max(rs => rs.TotalScore),
                        LatestReviewDate = g.Max(rs => rs.ReviewDate)
                    })
                    .OrderByDescending(u => u.TotalScore)
                    .ThenByDescending(u => u.LatestReviewDate)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < userScores.Count; i++)
                {
                    userScores[i].Rank = i + 1;
                }

                // Find current user's position
                var currentUser = userScores.FirstOrDefault(u => u.UserId == currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning($"No review scores found for user {currentUserId}");
                    return NotFound(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.NotFound,
                        Message = "No review scores found for current user"
                    });
                }

                _logger.LogInformation($"User {currentUserId} is at position {currentUser.Rank}");

                return Ok(new ApiResponse<LeaderboardDto>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "User leaderboard position fetched successfully",
                    Data = currentUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user leaderboard position: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"An error occurred while fetching user position: {ex.Message}",
                    Data = null
                });
            }
        }

        // ✅ Get leaderboard with detailed review scores
        [HttpGet("detailed")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DetailedLeaderboardDto>>>> GetDetailedLeaderboard()
        {
            try
            {
                _logger.LogInformation("Fetching detailed leaderboard data");

                // Get all scores first
                var allScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.User.FullName,
                        rs.User.ProfileImageUrl,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                var detailedData = allScores
                    .Select(rs => new
                    {
                        rs.UserId,
                        rs.FullName,
                        rs.ProfileImageUrl,
                        AcademicScore = rs.AcademicScore,
                        ReviewScore = rs.ReviewScoreValue,
                        TaskScore = rs.TaskScore,
                        TotalScore = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .Where(rs => rs.TotalScore > 0)
                    .GroupBy(rs => rs.UserId)
                    .Select(g => new DetailedLeaderboardDto
                    {
                        UserId = g.Key,
                        FullName = g.First().FullName ?? "Unknown User",
                        ProfileImageUrl = g.First().ProfileImageUrl,
                        TotalScore = g.Max(rs => rs.TotalScore),
                        AcademicScore = g.OrderByDescending(rs => rs.TotalScore).First().AcademicScore,
                        ReviewScore = g.OrderByDescending(rs => rs.TotalScore).First().ReviewScore,
                        TaskScore = g.OrderByDescending(rs => rs.TotalScore).First().TaskScore,
                        LatestReviewDate = g.Max(rs => rs.ReviewDate),
                        ReviewCount = g.Count()
                    })
                    .OrderByDescending(u => u.TotalScore)
                    .ThenByDescending(u => u.LatestReviewDate)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < detailedData.Count; i++)
                {
                    detailedData[i].Rank = i + 1;
                }

                _logger.LogInformation($"Returning detailed leaderboard with {detailedData.Count} users");

                return Ok(new ApiResponse<IEnumerable<DetailedLeaderboardDto>>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Detailed leaderboard data fetched successfully",
                    Data = detailedData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching detailed leaderboard data: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"An error occurred while fetching detailed leaderboard: {ex.Message}",
                    Data = null
                });
            }
        }

        // ✅ Alternative: Simple leaderboard without grouping
        [HttpGet("latest")]
        [Authorize(Roles = "Admin,Mentor,User")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaderboardDto>>>> GetLatestLeaderboard()
        {
            try
            {
                _logger.LogInformation("Fetching latest leaderboard data");

                // Get all scores first
                var allScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Select(rs => new
                    {
                        rs.Id,
                        rs.UserId,
                        rs.User.FullName,
                        rs.User.ProfileImageUrl,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                var latestScores = allScores
                    .Select(rs => new
                    {
                        rs.Id,
                        rs.UserId,
                        rs.FullName,
                        rs.ProfileImageUrl,
                        TotalScore = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .Where(rs => rs.TotalScore > 0)
                    .GroupBy(rs => rs.UserId)
                    .Select(g => g.OrderByDescending(rs => rs.ReviewDate).First())
                    .OrderByDescending(rs => rs.TotalScore)
                    .ThenByDescending(rs => rs.ReviewDate)
                    .Select(rs => new LeaderboardDto
                    {
                        UserId = rs.UserId,
                        FullName = rs.FullName ?? "Unknown User",
                        ProfileImageUrl = rs.ProfileImageUrl,
                        TotalScore = rs.TotalScore,
                        LatestReviewDate = rs.ReviewDate
                    })
                    .ToList();

                // Assign ranks
                for (int i = 0; i < latestScores.Count; i++)
                {
                    latestScores[i].Rank = i + 1;
                }

                _logger.LogInformation($"Returning latest leaderboard with {latestScores.Count} users");

                return Ok(new ApiResponse<IEnumerable<LeaderboardDto>>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Latest leaderboard data fetched successfully",
                    Data = latestScores
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest leaderboard data: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"An error occurred while fetching latest leaderboard: {ex.Message}",
                    Data = null
                });
            }
        }

        // ✅ Debug endpoint to check database state
        [HttpGet("debug")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> DebugDatabase()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalReviewScores = await _context.ReviewScores.CountAsync();

                // Get sample scores to check values
                var sampleScores = await _context.ReviewScores
                    .Include(rs => rs.User)
                    .Take(10)
                    .Select(rs => new
                    {
                        rs.Id,
                        rs.UserId,
                        UserName = rs.User.FullName,
                        rs.AcademicScore,
                        rs.ReviewScoreValue,
                        rs.TaskScore,
                        CalculatedTotal = rs.AcademicScore + rs.ReviewScoreValue + rs.TaskScore,
                        rs.ReviewDate
                    })
                    .ToListAsync();

                var usersWithScores = sampleScores
                    .Where(rs => rs.CalculatedTotal > 0)
                    .Select(rs => rs.UserId)
                    .Distinct()
                    .Count();

                return Ok(new ApiResponse<object>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Database debug information",
                    Data = new
                    {
                        TotalUsers = totalUsers,
                        TotalReviewScores = totalReviewScores,
                        UsersWithScores = usersWithScores,
                        SampleScores = sampleScores,
                        HasScoresWithTotalGreaterThanZero = sampleScores.Any(rs => rs.CalculatedTotal > 0)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug endpoint: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = $"Debug error: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}