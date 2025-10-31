namespace Bridgeon.Dtos.LeaveRequest
{
    public class LeaveRequestActionDto
    {
        public int RequestId { get; set; }
        public bool Approve { get; set; }
        public string? Notes { get; set; }
    }
}
