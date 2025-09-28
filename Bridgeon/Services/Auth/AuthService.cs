using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Models;
using Bridgeon.Repositeries.UserRepo;
using Bridgeon.Services.Token;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;




namespace Bridgeon.Services.Auth
{


    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepo, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest dto, string ipAddress)
        {
            var existing = await _userRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"   // All new users default User
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;

            user.RefreshTokens = new System.Collections.Generic.List<RefreshToken> { refreshToken };
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken.Token, AccessTokenExpires = atExpires };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest dto, string ipAddress)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            if (user.IsBlocked) throw new Exception("User is blocked");

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;

            user.RefreshTokens ??= new System.Collections.Generic.List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken.Token, AccessTokenExpires = atExpires };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) throw new Exception("Invalid token");

            var rt = user.RefreshTokens.First(t => t.Token == token);
            if (rt.IsRevoked || rt.Expires < DateTime.UtcNow) throw new Exception("Token invalid or expired");

            rt.IsRevoked = true;
            var newRt = _tokenService.CreateRefreshToken(ipAddress);
            newRt.UserId = user.Id;
            user.RefreshTokens.Add(newRt);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            return new AuthResponse { AccessToken = accessToken, RefreshToken = newRt.Token, AccessTokenExpires = atExpires };
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) return;

            var rt = user.RefreshTokens.First(t => t.Token == token);
            rt.IsRevoked = true;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
        }
    }
}
