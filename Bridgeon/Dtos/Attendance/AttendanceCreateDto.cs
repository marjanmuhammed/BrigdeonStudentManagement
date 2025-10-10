using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.Attendance
{
    public class AttendanceCreateDto
    {
        public string UserId { get; set; } // required
        public DateTime Date { get; set; } // date only ideally
        public AttendanceStatus Status { get; set; }
        public string Notes { get; set; }
    }
}
