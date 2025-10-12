namespace Bridgeon.Dtos.CreateReview
{
    public class PendingFeesDto
    {
        public string FeeCategory { get; set; }
        public decimal PendingAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string FeeStatus { get; set; } = "Pending";
    }
}
