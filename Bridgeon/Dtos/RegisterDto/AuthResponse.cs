namespace Bridgeon.Dtos.RegisterDto
{
    public class AuthResponse
    {
        public string Email { get; set; }       // user email
        public string FullName { get; set; }    // user full name
        public string Role { get; set; }        // user role

        // Tokens only for internal use, cookies store them
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public DateTime AccessTokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
    }
}
