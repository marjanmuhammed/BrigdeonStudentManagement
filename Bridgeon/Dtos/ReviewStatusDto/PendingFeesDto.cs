namespace Bridgeon.Dtos.ReviewStatusDto
{
    public class PendingFeesDto
    {
        public int UserId { get; set; }                // Which user
        public string FeeCategory { get; set; }       // e.g., "Weekback", "Monthly", "Training"
        public decimal PendingAmount { get; set; }    // Pending amount
        public DateTime? DueDate { get; set; }        // Due date
        public string FeeStatus { get; set; } = "Pending"; // Pending / Paid
    }
}
