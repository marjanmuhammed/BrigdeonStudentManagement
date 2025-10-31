using Bridgeon.Models;
using Bridgeon.Repositeries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridgeon.Data;
using Bridgeon.Models;

namespace Bridgeon.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly AppDbContext _context;

        public HolidayRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            return await _context.Holidays
                        .OrderBy(h => h.Date)
                        .ToListAsync();
        }

        public async Task<Holiday> GetByIdAsync(int id)
        {
            return await _context.Holidays.FindAsync(id);
        }

        public async Task<Holiday> GetByDateAsync(DateTime date)
        {
            var d = date.Date;
            return await _context.Holidays.FirstOrDefaultAsync(h => h.Date.Date == d);
        }

        public async Task AddAsync(Holiday holiday)
        {
            await _context.Holidays.AddAsync(holiday);
        }

        public void Update(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
        }

        public void Remove(Holiday holiday)
        {
            _context.Holidays.Remove(holiday);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}