using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.RegisterDto
{
    public class RegisterRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }
    }
}
