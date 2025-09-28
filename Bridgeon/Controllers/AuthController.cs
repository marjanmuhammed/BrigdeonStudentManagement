using Azure;
using Azure.Core;
using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Services.Auth;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.RegisterAsync(dto, ip);

            // ------------------- Cookies -------------------
            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // adjust days same as config
            });

            return StatusCode(ApiStatusCodes.Created, new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Created,
                Message = "Registration successful",
                Data = new AuthResponse
                {
                    Email = res.Email,
                    FullName = res.FullName,
                    Role = res.Role
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Status = ApiStatusCodes.BadRequest,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.LoginAsync(dto, ip);

            // ------------------- Cookies -------------------
            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // adjust days same as config
            });

            return Ok(new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Success,
                Message = "Login successful",
                Data = new AuthResponse
                {
                    Email = res.Email,
                    FullName = res.FullName,
                    Role = res.Role
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new ApiResponse<string>
            {
                Status = ApiStatusCodes.Unauthorized,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token found");

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _authService.RefreshTokenAsync(refreshToken, ip);

            Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpires
            });

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // adjust as needed
            });

            return Ok(new ApiResponse<AuthResponse>
            {
                Status = ApiStatusCodes.Success,
                Message = "Token refreshed",
                Data = new AuthResponse
                {
                    Email = res.Email,
                    FullName = res.FullName,
                    Role = res.Role
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new ApiResponse<string>
            {
                Status = ApiStatusCodes.Unauthorized,
                Message = ex.Message,
                Data = null
            });
        }
    }

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
            return StatusCode(ApiStatusCodes.ServerError, new ApiResponse<string>
            {
                Status = ApiStatusCodes.ServerError,
                Message = ex.Message,
                Data = null
            });
        }
    }
}
