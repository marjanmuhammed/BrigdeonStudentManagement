namespace Bridgeon.Dtos
{
    public class CreateReviewDto
    {
        public int UserId { get; set; }
        public string ReviewStatus { get; set; } = "Not Assigned";
        public string? ReviewDate { get; set; }
    }
}