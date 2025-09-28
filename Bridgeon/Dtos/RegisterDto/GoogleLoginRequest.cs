using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.RegisterDto
{
    public class GoogleLoginRequest
    {
        [Required]
        public string Email { get; set; }
    }
}
