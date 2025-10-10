using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.Attendance
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string Notes { get; set; }
        public string RecordedById { get; set; }
    }
}
