using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.Attendance
{
    public class AttendanceCreateDto
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
