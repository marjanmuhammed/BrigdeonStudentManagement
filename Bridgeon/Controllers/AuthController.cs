using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Services.Auth;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Default: all endpoints require authentication
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

        private CookieOptions GetCookieOptions(DateTime expires)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // In dev, can make false if needed
                SameSite = SameSiteMode.None,
                Expires = expires
            };
        }

        // -------------------- LOGIN --------------------
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.BadRequest,
                        Message = "Email and password are required",
                        Data = null
                    });

                _logger.LogInformation("Login attempt: {Email}", dto.Email);

                var user = await _authService.GetByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "Email not found",
                        Data = null
                    });

                if (user.PasswordHash == null)
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "Account not set up. Please register first.",
                        Data = null
                    });

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "Invalid password",
                        Data = null
                    });

                if (user.IsBlocked)
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Forbidden,
                        Message = "Your account is blocked",
                        Data = null
                    });

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                var authResponse = await _authService.LoginAsync(dto, ip);

                Response.Cookies.Append("accessToken", authResponse.AccessToken, GetCookieOptions(authResponse.AccessTokenExpires));
                Response.Cookies.Append("refreshToken", authResponse.RefreshToken, GetCookieOptions(authResponse.RefreshTokenExpires));

                return Ok(new ApiResponse<AuthResponse>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Login successful",
                    Data = authResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", dto?.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An unexpected error occurred during login",
                    Data = IsDevelopment ? ex.Message : null
                });
            }
        }

        // -------------------- REGISTER --------------------
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", dto.Email);

                // ✅ Validate email
                if (string.IsNullOrEmpty(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.BadRequest,
                        Message = "Please provide a valid email address",
                        Data = null
                    });

                // ✅ Validate password
                if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.BadRequest,
                        Message = "Password must be at least 6 characters long",
                        Data = null
                    });

                // ✅ Check if email exists in DB
                var user = await _authService.GetByEmailAsync(dto.Email);

                if (user != null)
                {
                    if (user.PasswordHash != null)
                    {
                        // Email exists & already registered
                        return BadRequest(new ApiResponse<string>
                        {
                            Status = ApiStatusCodes.BadRequest,
                            Message = "ALREADY_REGISTERED",
                            Data = null
                        });
                    }
                    // Email exists but password null → continue to registration
                }
                else
                {
                    // Email not found in DB → cannot register
                    return NotFound(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.NotFound,
                        Message = "EMAIL_NOT_IN_SYSTEM",
                        Data = null
                    });
                }

                // ✅ Normal registration (new email or existing email with null password)
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _authService.RegisterAsync(dto, ip); // AuthService should handle null-password user update

                Response.Cookies.Append("accessToken", res.AccessToken, GetCookieOptions(res.AccessTokenExpires));
                Response.Cookies.Append("refreshToken", res.RefreshToken, GetCookieOptions(res.RefreshTokenExpires));

                return Ok(new ApiResponse<AuthResponse>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Registration successful",
                    Data = res
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for: {Email}", dto?.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An error occurred during registration",
                    Data = IsDevelopment ? ex.Message : null
                });
            }
        }

        // -------------------- REFRESH TOKEN --------------------
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.Unauthorized,
                        Message = "No refresh token found",
                        Data = null
                    });

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _authService.RefreshTokenAsync(refreshToken, ip);

                Response.Cookies.Append("accessToken", res.AccessToken, GetCookieOptions(res.AccessTokenExpires));
                Response.Cookies.Append("refreshToken", res.RefreshToken, GetCookieOptions(res.RefreshTokenExpires));

                return Ok(new ApiResponse<AuthResponse>
                {
                    Status = ApiStatusCodes.Success,
                    Message = "Token refreshed successfully",
                    Data = res
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.Unauthorized,
                    Message = "Token refresh failed",
                    Data = IsDevelopment ? ex.Message : null
                });
            }
        }

        // -------------------- LOGOUT / REVOKE --------------------
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
                    Message = "Logged out successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An unexpected error occurred while logging out",
                    Data = IsDevelopment ? ex.Message : null
                });
            }
        }

        // -------------------- DEBUG COOKIES --------------------
        [AllowAnonymous]
        [HttpGet("debug-cookies")]
        public IActionResult DebugCookies()
        {
            var allCookies = Request.Cookies.Keys.ToDictionary(k => k, k => Request.Cookies[k]);
            _logger.LogInformation("Cookies received: {@Cookies}", allCookies);

            return Ok(new
            {
                hasAccessToken = Request.Cookies.ContainsKey("accessToken"),
                hasRefreshToken = Request.Cookies.ContainsKey("refreshToken"),
                allCookies
            });
        }

        // -------------------- DEBUG TOKEN --------------------
        [Authorize]
        [HttpGet("debug-token")]
        public IActionResult DebugToken()
        {
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(new
            {
                message = "Token is valid!",
                claims,
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(ClaimTypes.Email)?.Value
            });
        }

        // -------------------- TEST CORS --------------------
        [AllowAnonymous]
        [HttpGet("test-cors")]
        public IActionResult TestCors()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            return Ok(new
            {
                message = "CORS test successful",
                headers,
                origin = Request.Headers["Origin"].ToString(),
                hasCredentials = true
            });
        }

        // -------------------- CHECK EMAIL --------------------
        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new ApiResponse<string>
                {
                    Status = ApiStatusCodes.BadRequest,
                    Message = "Email is required",
                    Data = null
                });

            try
            {
                var user = await _authService.GetByEmailAsync(email);
                if (user == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Status = ApiStatusCodes.NotFound,
                        Message = "Email not found",
                        Data = null
                    });

                return Ok(new ApiResponse<object>
                {
                    Status = ApiStatusCodes.Success,
                    Message = user.PasswordHash == null
                        ? "⚠️ Email verified but not registered yet"
                        : "✅ Email exists and active",
                    Data = new
                    {
                        email = user.Email,
                        passwordExists = user.PasswordHash != null,
                        role = user.Role,
                        isBlocked = user.IsBlocked
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email: {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Status = ApiStatusCodes.ServerError,
                    Message = "An error occurred while checking email",
                    Data = IsDevelopment ? ex.Message : null
                });
            }
        }
    }
}
