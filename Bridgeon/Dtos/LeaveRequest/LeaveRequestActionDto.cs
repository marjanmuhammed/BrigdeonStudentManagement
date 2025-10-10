namespace Bridgeon.Dtos.LeaveRequest
{
    public class LeaveRequestActionDto
    {
        public int RequestId { get; set; }
        public bool Approve { get; set; } // true = approve; false = reject
        public string Notes { get; set; } // optional review notes
    }
}
