namespace Bridgeon.Dtos.LeaveRequest
{
    public class LeaveRequestCreateDto
    {
        public DateTime Date { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string? ProofImageUrl { get; set; }
    }
}
