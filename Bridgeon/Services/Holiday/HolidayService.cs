using Bridgeon.Dtos;
using Bridgeon.Models;
using Bridgeon.Repositeries;
using Bridgeon.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridgeon.Dtos;
using Bridgeon.Models;
using Bridgeon.Repositories;

namespace Bridgeon.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _repo;

        public HolidayService(IHolidayRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<HolidayReadDto>> GetAllAsync()
        {
            var holidays = await _repo.GetAllAsync();
            return holidays.Select(h => new HolidayReadDto
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name,
                Description = h.Description
            });
        }

        public async Task<HolidayReadDto> GetByIdAsync(int id)
        {
            var h = await _repo.GetByIdAsync(id);
            if (h == null) return null;
            return new HolidayReadDto
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name,
                Description = h.Description
            };
        }

        public async Task<HolidayReadDto> CreateAsync(HolidayCreateDto dto)
        {
            // Business rule: do not allow duplicate date
            var existing = await _repo.GetByDateAsync(dto.Date);
            if (existing != null)
                throw new InvalidOperationException("Holiday already exists on this date.");

            var entity = new Holiday
            {
                Date = dto.Date.Date,
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            var saved = await _repo.SaveChangesAsync();
            if (!saved) throw new Exception("Failed to save holiday.");

            return new HolidayReadDto
            {
                Id = entity.Id,
                Date = entity.Date,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task<bool> UpdateAsync(int id, HolidayUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            // if date changed check conflict
            if (entity.Date.Date != dto.Date.Date)
            {
                var conflict = await _repo.GetByDateAsync(dto.Date);
                if (conflict != null && conflict.Id != id)
                    throw new InvalidOperationException("Another holiday exists on the provided date.");
            }

            entity.Date = dto.Date.Date;
            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            _repo.Remove(entity);
            return await _repo.SaveChangesAsync();
        }
    }
}