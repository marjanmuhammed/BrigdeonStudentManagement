using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridgeon.Models;



namespace Bridgeon.Repositeries
{
    public interface IHolidayRepository
    {
        Task<IEnumerable<Holiday>> GetAllAsync();
        Task<Holiday> GetByIdAsync(int id);
        Task<Holiday> GetByDateAsync(DateTime date);
        Task AddAsync(Holiday holiday);
        void Update(Holiday holiday);
        void Remove(Holiday holiday);
        Task<bool> SaveChangesAsync();
    }
}
