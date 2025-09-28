﻿using Bridgeon.Models;

namespace Bridgeon.Repositeries.UserRepo
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        void Update(User user);
        void Remove(User user);
        Task SaveChangesAsync();
    }
}
