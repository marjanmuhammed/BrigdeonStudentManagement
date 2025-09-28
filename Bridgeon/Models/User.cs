using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }  // store hashed

        public bool IsBlocked { get; set; } = false;

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public string Role { get; set; } = "User"; // Default role
    }
}
