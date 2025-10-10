using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.UserProfile
{
    public class ChangePasswordDto
    {

        [Required]
        public string CurrentPassword { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}

