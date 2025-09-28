using Bridgeon.Data;
using Bridgeon.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Bridgeon.Repositeries.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

        public async Task<User> GetByIdAsync(int id) =>
            await _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User> GetByEmailAsync(string email) =>
            await _db.Users.Include(u => u.RefreshTokens)
                           .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _db.Users.Include(u => u.RefreshTokens).ToListAsync();

        public void Update(User user) => _db.Users.Update(user);

        public void Remove(User user) => _db.Users.Remove(user);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
