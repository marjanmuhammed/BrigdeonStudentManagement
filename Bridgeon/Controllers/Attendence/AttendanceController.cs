using Bridgeon.Dtos.Attendance;
using Bridgeon.Services;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        // Create attendance
        [HttpPost]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Create([FromBody] AttendanceCreateDto dto)
        {
            try
            {
                var recordedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var res = await _service.CreateAttendanceAsync(dto, recordedBy);

                var response = new ApiResponse<AttendanceDto>
                {
                    Status = ApiStatusCodes.Created,
                    Message = "Attendance created successfully",
                    Data = res
                };

                return StatusCode(ApiStatusCodes.Created, response);
            }
            catch (InvalidOperationException ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(errorResponse);
            }
        }

        // Update attendance by userId and date
        [HttpPut]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Update([FromBody] AttendanceUpdateDto dto)
        {
            try
            {
                var modifiedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var res = await _service.UpdateAttendanceAsync(dto, modifiedBy);

                return Ok(new ApiResponse<AttendanceDto>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Attendance updated successfully",
                    Data = res
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An error occurred while updating attendance",
                    Data = null
                });
            }
        }

        // Delete attendance by userId and date
        [HttpDelete]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Delete([FromQuery] int userId, [FromQuery] DateTime date)
        {
            try
            {
                var ok = await _service.DeleteAttendanceAsync(userId, date.Date);

                if (!ok)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.NotFound,
                        Message = "Attendance record not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Attendance deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An error occurred while deleting attendance",
                    Data = null
                });
            }
        }

        // Get attendance by userId and date
        [HttpGet("user-date")]
        [Authorize]
        public async Task<IActionResult> GetByUserAndDate([FromQuery] int userId, [FromQuery] DateTime date)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdminOrMentor = User.IsInRole("Admin") || User.IsInRole("Mentor");

            if (!isAdminOrMentor && userId != currentUserId)
            {
                return StatusCode(ApiStatusCodes.Forbidden, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Forbidden,
                    Message = "You are not allowed to access this resource",
                    Data = null
                });
            }

            var attendance = await _service.GetByUserAndDateAsync(userId, date.Date);

            if (attendance == null)
                return NotFound(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.NotFound,
                    Message = "Attendance not found",
                    Data = null
                });

            return Ok(new ApiResponse<AttendanceDto>
            {
                Status = ApiStatusCodes.Success,
                Message = "Attendance fetched successfully",
                Data = attendance
            });
        }

        // Get all attendances (Admin/Mentor only)
        [HttpGet]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(new ApiResponse<object>
            {
                Status = ApiStatusCodes.Success,
                Message = "All attendances fetched successfully",
                Data = items
            });
        }

        // Get user's own attendances
        [HttpGet("my-attendances")]
        [Authorize]
        public async Task<IActionResult> GetMyAttendances()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Unauthorized,
                    Message = "Unauthorized",
                    Data = null
                });

            var items = await _service.GetUserAttendancesAsync(userId);
            return Ok(new ApiResponse<object>
            {
                Status = ApiStatusCodes.Success,
                Message = "Your attendances fetched successfully",
                Data = items
            });
        }

        // Get attendances for specific user (Admin/Mentor only)
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> GetUserAttendances(int userId)
        {
            var items = await _service.GetUserAttendancesAsync(userId);
            return Ok(new ApiResponse<object>
            {
                Status = ApiStatusCodes.Success,
                Message = "User attendances fetched successfully",
                Data = items
            });
        }

        // Calendar query: get attendances for a given month/year
        [HttpGet("calendar")]
        [Authorize]
        public async Task<IActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
        {
            if (year <= 0 || month < 1 || month > 12)
                return BadRequest(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Invalid year/month",
                    Data = null
                });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdminOrMentor = User.IsInRole("Admin") || User.IsInRole("Mentor");

            var items = await _service.GetMonthAsync(year, month, isAdminOrMentor ? (int?)null : userId);

            return Ok(new ApiResponse<object>
            {
                Status = ApiStatusCodes.Success,
                Message = "Attendances fetched for the month successfully",
                Data = items
            });
        }
    }
}