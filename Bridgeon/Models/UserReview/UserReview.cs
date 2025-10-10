
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridgeon.Models
{
    public class UserReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string ReviewStatus { get; set; } = "Not Assigned"; // Values: "Not Assigned", "Assigned", "Completed"

        public DateTime? ReviewDate { get; set; } // New review date set by mentor

        // Pending Fees properties
        [StringLength(50)]
        public string? FeeCategory { get; set; } // e.g., "Weekback", "Monthly", "Training"

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PendingAmount { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(20)]
        public string? FeeStatus { get; set; } = "Pending"; // Pending, Paid

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}