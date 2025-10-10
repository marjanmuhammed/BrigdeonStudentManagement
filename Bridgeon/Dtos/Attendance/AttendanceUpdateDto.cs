using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.Attendance
{
    public class AttendanceUpdateDto
    {
        public AttendanceStatus Status { get; set; }
        public string Notes { get; set; }
    }
}
