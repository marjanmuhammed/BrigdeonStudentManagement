using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Services.Auth;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IWebHostEnvironment env)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
    }

    private bool IsDevelopment => _env.IsDevelopment();

    // -------------------- Login --------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        try
        {
            _logger.LogInformation("Login attempt for: {Email}", dto.Email);

            var user = await _authService.GetByEmailAsync(dto.Email);
            if (user == null) 
            {
                _logger.LogWarning("Login failed - email not found: {Email}", dto.Email);
                return Unauthorized(new { message = "Email not found" });
            }
            
            if (user.PasswordHash == null) 
            {
                _logger.LogWarning("Login failed - account not set up: {Email}", dto.Email);
                return Unauthorized(new { message = "Account not set up. Please register first." });
            }
            
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) 
            {
                _logger.LogWarning("Login failed - invalid password for: {Email}", dto.Email);
                return Unauthorized(new { message = "Invalid password" });
            }
            
            if (user.IsBlocked) 
            {
                _logger.LogWarning("Login failed - account blocked: {Email}", dto.Email);
                return StatusCode(403, new { message = "Your account is blocked" });
            }

            var authResponse = await _authService.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            // Set cookies with development-friendly settings
            Response.Cookies.Append("accessToken", authResponse.AccessToken, new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = !IsDevelopment, // false in development, true in production
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = authResponse.AccessTokenExpires 
            });
            
            Response.Cookies.Append("refreshToken", authResponse.RefreshToken, new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = !IsDevelopment, // false in development, true in production
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = authResponse.RefreshTokenExpires 
            });

            _logger.LogInformation("Login successful for: {Email}", dto.Email);
            return Ok(new { status = 200, message = "Login successful", data = authResponse });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for: {Email}", dto.Email);
            return Unauthorized(new { message = ex.Message });
        }
    }

    // -------------------- Single Step Registration --------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
    {
        try
        {
            _logger.LogInformation("Registration attempt for: {Email}", dto.Email);

            // Validate email format
            if (string.IsNullOrEmpty(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Please provide a valid email address",
                    Data = null
                });
            }

            // Validate password
            if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Password must be at least 6 characters long",
                    Data = null
                });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.RegisterAsync(dto, ip);

            // Set cookies with development-friendly settings
            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevelopment, // false in development, true in production
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevelopment, // false in development, true in production
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = res.RefreshTokenExpires
            });

            return Ok(new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Success,
                Message = "Registration successful",
                Data = res
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for: {Email}", dto.Email);
            return BadRequest(new ApiResponse<string>
            {
                Status = ApiStatusCodes.BadRequest,
                Message = ex.Message,
                Data = null
            });
        }
    }

    // -------------------- Google Login --------------------
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Email is required",
                    Data = null
                });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.GoogleLoginAsync(dto.Email, ip);

            // Set cookies with development-friendly settings
            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite =  SameSiteMode.None,
                Expires = res.RefreshTokenExpires
            });

            return Ok(new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Success,
                Message = "Google login successful",
                Data = res
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed for: {Email}", dto.Email);
            return Unauthorized(new ApiResponse<string>
            {
                Status = ApiStatusCodes.Unauthorized,
                Message = ex.Message,
                Data = null
            });
        }
    }

    // -------------------- Refresh Token --------------------
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token not found in cookies");
                return Unauthorized("No refresh token found");
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.RefreshTokenAsync(refreshToken, ip);

            // Set cookies with development-friendly settings
            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevelopment,
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevelopment,
                SameSite = IsDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = res.RefreshTokenExpires
            });

            return Ok(new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Success,
                Message = "Token refreshed",
                Data = res
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(new ApiResponse<string>
            {
                Status = ApiStatusCodes.Unauthorized,
                Message = ex.Message,
                Data = null
            });
        }
    }

    // -------------------- Revoke / Logout --------------------
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _authService.RevokeRefreshTokenAsync(refreshToken, ip);
            }

            // Delete cookies
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new ApiResponse<string>
            {
                Status = ApiStatusCodes.Success,
                Message = "Logged out / tokens revoked",
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(ApiStatusCodes.ServerError, new ApiResponse<string>
            {
                Status = ApiStatusCodes.ServerError,
                Message = ex.Message,
                Data = null
            });
        }
    }
}