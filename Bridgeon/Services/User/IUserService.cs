using Bridgeon.Models;

namespace Bridgeon.Services
{
    public interface IUserService
    {
        User GetById(int id);
        User GetByEmail(string email);
    }
}
