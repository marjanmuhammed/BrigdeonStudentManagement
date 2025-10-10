namespace Bridgeon.Dtos.ReviewStatusDto
{
    public class ReviewStatusDto
    {
        public int UserId { get; set; }                 // Which user
        public string ReviewStatus { get; set; } = "Not Assigned"; // Assigned / Not Assigned
        public DateTime? ReviewDate { get; set; }      // Next review date set by mentor
    }
}
