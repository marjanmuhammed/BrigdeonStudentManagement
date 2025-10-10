using Bridgeon.Dtos.Attendance;
using Bridgeon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgeon.DTOs;
using Bridgeon.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        public AttendanceController(IAttendanceService service) { _service = service; }

        // Admin/Mentor create attendance
        [HttpPost]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Create([FromBody] AttendanceCreateDto dto)
        {
            try
            {
                var recordedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var res = await _service.CreateAttendanceAsync(dto, recordedBy);
                return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            // call service (you can add GetById service if needed)
            // For brevity, you can reuse month API to fetch single if service expanded.
            return Ok(); // implement if needed
        }

        // Update
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Update(int id, [FromBody] AttendanceUpdateDto dto)
        {
            var modifiedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var res = await _service.UpdateAttendanceAsync(id, dto, modifiedBy);
            if (res == null) return NotFound();
            return Ok(res);
        }

        // Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAttendanceAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Calendar query: get attendances for a given month/year (for UI calendar)
        [HttpGet("calendar")]
        [Authorize]
        public async Task<IActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
        {
            if (year <= 0 || month < 1 || month > 12)
                return BadRequest("Invalid year/month");

            var items = await _service.GetMonthAsync(year, month);
            return Ok(items);
        }

        // Get user range (for previous/next year checks)
        [HttpGet("user/{userId}/range")]
        [Authorize]
        public async Task<IActionResult> GetUserRange(string userId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var items = await _service.GetUserRangeAsync(userId, from, to);
            return Ok(items);
        }
    }
}
