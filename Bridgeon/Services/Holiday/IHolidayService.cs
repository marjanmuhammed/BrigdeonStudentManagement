using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridgeon.Dtos;


namespace Bridgeon.Services
{
    public interface IHolidayService
    {
        Task<IEnumerable<HolidayReadDto>> GetAllAsync();
        Task<HolidayReadDto> GetByIdAsync(int id);
        Task<HolidayReadDto> CreateAsync(HolidayCreateDto dto);
        Task<bool> UpdateAsync(int id, HolidayUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
