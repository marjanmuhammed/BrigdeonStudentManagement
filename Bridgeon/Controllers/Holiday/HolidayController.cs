// Controllers/HolidaysController.cs
using Bridgeon.Dtos;
using Bridgeon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Bridgeon.Dtos;
using Bridgeon.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidayService _service;

        public HolidaysController(IHolidayService service)
        {
            _service = service;
        }

        // GET: api/Holidays
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // GET: api/Holidays/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var h = await _service.GetByIdAsync(id);
            if (h == null) return NotFound();
            return Ok(h);
        }

        // POST: api/Holidays
        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HolidayCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ioe)
            {
                return Conflict(new { message = ioe.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unable to create holiday", detail = ex.Message });
            }
        }

        // PUT: api/Holidays/5
        // [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] HolidayUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ioe)
            {
                return Conflict(new { message = ioe.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unable to update holiday", detail = ex.Message });
            }
        }

        // DELETE: api/Holidays/5
        // [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
