using BCrypt.Net;
using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Models;
using Bridgeon.Repositeries.UserRepo;
using Bridgeon.Services.Token;

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

        public async Task<User?> GetByEmailAsync(string email) => await _userRepo.GetByEmailAsync(email);

        public async Task<AuthResponse> RegisterAsync(RegisterRequest dto, string ipAddress)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            // Check if email exists in system (admin-added)
            if (user == null)
                throw new Exception("Email not found in system. Please contact admin to add your email.");

            // Check if user is whitelisted
            if (!user.IsWhitelisted)
                throw new Exception("This email is not authorized to register. Contact admin.");

            // Check if user is already registered (has password)
            if (!string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("This email is already registered. Please login.");

            // Set password and complete registration
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;

            user.RefreshTokens = new List<RefreshToken> { refreshToken };
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpires = atExpires,
                RefreshTokenExpires = refreshToken.Expires,
                Email = user.Email,
                Role = user.Role,
                IsBlocked = user.IsBlocked
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest dto, string ipAddress)
{
    Console.WriteLine($"Login attempt for email: {dto.Email}");

    var user = await _userRepo.GetByEmailAsync(dto.Email);

    if (user == null)
    {
        Console.WriteLine("User not found in database");
        throw new Exception("Email not found. Please check your email or register first.");
    }

    Console.WriteLine($"User found: {user.Email}, PasswordHash null: {user.PasswordHash == null}");

    if (user.PasswordHash == null)
    {
        Console.WriteLine("User has no password set");
        throw new Exception("Account not set up. Please register first.");
    }

    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
    Console.WriteLine($"Password verification result: {isPasswordValid}");

    if (!isPasswordValid)
    {
        Console.WriteLine("Password verification failed");
        throw new Exception("Invalid password. Please try again.");
    }

    if (user.IsBlocked)
    {
        Console.WriteLine("User is blocked");
        throw new Exception("Your account has been blocked. Please contact admin.");
    }

    Console.WriteLine("Login successful, generating tokens...");

    var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
    var refreshToken = _tokenService.CreateRefreshToken(ipAddress);
    refreshToken.UserId = user.Id;

    user.RefreshTokens ??= new List<RefreshToken>();
    user.RefreshTokens.Add(refreshToken);

    _userRepo.Update(user);
    await _userRepo.SaveChangesAsync();

    return new AuthResponse
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken.Token,
        AccessTokenExpires = atExpires,
        RefreshTokenExpires = refreshToken.Expires,
        Email = user.Email,
        Role = user.Role,
        IsBlocked = user.IsBlocked
    };
}

        public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) throw new Exception("Invalid token");

            var rt = user.RefreshTokens.First(t => t.Token == token);
            if (rt.IsRevoked || rt.Expires < DateTime.UtcNow)
                throw new Exception("Token invalid or expired");

            rt.IsRevoked = true;
            var newRt = _tokenService.CreateRefreshToken(ipAddress);
            newRt.UserId = user.Id;
            user.RefreshTokens.Add(newRt);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRt.Token,
                AccessTokenExpires = atExpires,
                RefreshTokenExpires = newRt.Expires,
                Email = user.Email,
                Role = user.Role,
                IsBlocked = user.IsBlocked
            };
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

        public async Task<AuthResponse?> GoogleLoginAsync(string email, string ipAddress)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !user.IsWhitelisted)
                throw new Exception("This email cannot register. Contact admin.");

            if (!string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("Please use email and password to login");

            if (user.IsBlocked)
                throw new Exception("User is blocked");

            var accessToken = _tokenService.CreateAccessToken(user, out DateTime atExpires);
            var refreshToken = _tokenService.CreateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;

            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpires = atExpires,
                RefreshTokenExpires = refreshToken.Expires,
                Email = user.Email,
                Role = user.Role,
                IsBlocked = user.IsBlocked
            };
        }

        public void UpdateUser(User user) => _userRepo.Update(user);
        public async Task SaveChangesAsync() => await _userRepo.SaveChangesAsync();
    }
}