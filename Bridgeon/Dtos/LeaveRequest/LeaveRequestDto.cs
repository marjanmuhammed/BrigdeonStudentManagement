using Bridgeon.Models.Attendence;

namespace Bridgeon.Dtos.LeaveRequest
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime Date { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string ProofImageUrl { get; set; }
        public LeaveRequestStatus Status { get; set; }

        public int? ReviewedById { get; set; }
        public string? ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
