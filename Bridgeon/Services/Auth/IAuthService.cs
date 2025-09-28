using Bridgeon.Dtos.RegisterDto;

using Bridgeon.Models;

namespace Bridgeon.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<AuthResponse> RegisterAsync(RegisterRequest dto, string ipAddress);
        Task<AuthResponse> LoginAsync(LoginRequest dto, string ipAddress);
        Task<AuthResponse> GoogleLoginAsync(string email, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
        void UpdateUser(User user);
        Task SaveChangesAsync();
    }
}
