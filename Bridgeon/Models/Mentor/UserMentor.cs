using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models.Mentor
{
    public class UserMentor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MentorId { get; set; } // The mentor user ID

        [Required]
        public int MenteeId { get; set; } // The mentee user ID

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
