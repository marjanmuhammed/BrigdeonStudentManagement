using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.UserProfile
{
    public class UserProfileUpdateDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }
    }
}
