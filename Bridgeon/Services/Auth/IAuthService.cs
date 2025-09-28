using Bridgeon.Dtos.RegisterDto;
using System.Threading.Tasks;

namespace Bridgeon.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest dto, string ipAddress);
        Task<AuthResponse> LoginAsync(LoginRequest dto, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
    }
}
