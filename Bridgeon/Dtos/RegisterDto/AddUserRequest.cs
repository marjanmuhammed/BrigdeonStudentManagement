using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.RegisterDto
{
    public class AddUserRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; } // Admin sets role
    }

}
