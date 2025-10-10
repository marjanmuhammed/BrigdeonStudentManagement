using Bridgeon.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bridgeon.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        // ✅ Create Access Token
        public string CreateAccessToken(User user, out DateTime expires)
        {
            var jwtSection = _config.GetSection("Jwt");

            // 🔐 1. Create signing key and credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 🔐 2. Set token expiry
            var expiresInMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "30");
            expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            // 🔐 3. Add essential claims (UserId, Name, Email, Role)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            // 🔐 4. Create token
            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // ✅ Return the encoded token string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Create Refresh Token
        public RefreshToken CreateRefreshToken(string ipAddress)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7")
                ),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        // ✅ Validate Access Token
        public bool ValidateAccessToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
