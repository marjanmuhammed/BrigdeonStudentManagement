using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridgeon.Models.Attendence
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        // Id of the user for whom attendance is recorded
        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime Date { get; set; } // Date part is important; store as UTC or local consistently

        [Required]
        public AttendanceStatus Status { get; set; }

        public string Notes { get; set; } // optional notes e.g. "arrived at 10:30"

        public string RecordedById { get; set; } // admin/mentor who recorded

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
