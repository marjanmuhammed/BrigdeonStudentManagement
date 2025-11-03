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


        public string? PasswordHash { get; set; }  // store hashed

        public bool IsBlocked { get; set; } = false;

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        [Required]
        public string Role { get; set; }

        public bool IsWhitelisted { get; set; }

        public string? ProfileImageUrl { get; set; }

        //relation 1 to one
        public Profile Profile { get; set; }



        public int? MentorId { get; set; }   // Foreign Key to another User
        public User? Mentor { get; set; }    // Mentor Navigation
        public ICollection<User> Mentees { get; set; } = new List<User>(); // Mentees list

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
