using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string CreatedByIp { get; set; }

        // relation
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
