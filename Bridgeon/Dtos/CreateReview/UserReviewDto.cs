namespace Bridgeon.Dtos.CreateReview
{
    public class UserReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ReviewStatus { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? FeeCategory { get; set; }
        public decimal? PendingAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? FeeStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // User details
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
    }
}
