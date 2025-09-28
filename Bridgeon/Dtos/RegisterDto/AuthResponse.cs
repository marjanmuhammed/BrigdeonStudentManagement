namespace Bridgeon.Dtos.RegisterDto
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpires { get; set; }

        // Added fields for client
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }

        public DateTime RefreshTokenExpires { get; set; } // <<< ADD THIS
    }
}
