using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridgeon.Models.Attendence
{

    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan CheckInTime { get; set; }

     
        public TimeSpan? CheckOutTime { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public int RecordedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
