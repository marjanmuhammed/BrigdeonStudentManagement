using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos.RegisterDto
{
    public class UpdateUserRoleRequest
    {
        [Required]
        public string Role { get; set; } // Admin/User/Mentor
    }
}
