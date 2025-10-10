using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.LeaveRequest
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public string ReviewedById { get; set; }
        public string ReviewNotes { get; set; }
    }
}
