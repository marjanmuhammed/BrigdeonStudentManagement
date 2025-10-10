using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.UserProfile
{
    public class UpdateProfileImageDto
    {
        [Required]
        public string ImageUrl { get; set; }
    }
}
