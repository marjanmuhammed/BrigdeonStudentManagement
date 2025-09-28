using Bridgeon.Models;

namespace Bridgeon.Services.Token
{
    public interface ITokenService
    {
        string CreateAccessToken(User user, out DateTime expires);
        RefreshToken CreateRefreshToken(string ipAddress);
    }
}
