using System;
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

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [StringLength(50)]
        public string ReviewStatus { get; set; } = "Not Assigned";

        public DateTime? ReviewDate { get; set; }

        // Fee related properties
        [StringLength(100)]
        public string? FeeCategory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PendingAmount { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(50)]
        public string? FeeStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}