using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models.Attendence
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // requester

        [Required]
        public DateTime Date { get; set; } // date the user requests leave for

        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string? ProofImageUrl { get; set; }

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        public int? ReviewedById { get; set; }

        public string? ReviewNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

}
