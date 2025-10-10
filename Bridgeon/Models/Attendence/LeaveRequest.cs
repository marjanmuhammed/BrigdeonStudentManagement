using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models.Attendence
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // requester

        [Required]
        public DateTime Date { get; set; } // date the user requests leave for

        public string Reason { get; set; }

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        public string ReviewedById { get; set; } // mentor/admin who approved/rejected

        public string ReviewNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
