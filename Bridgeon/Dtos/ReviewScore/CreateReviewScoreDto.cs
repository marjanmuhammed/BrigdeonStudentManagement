namespace Bridgeon.Dtos
{
    public class CreateReviewScoreDto
    {
        public int UserId { get; set; }
        public int Week { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewerName { get; set; }
        public decimal AcademicScore { get; set; }
        public decimal ReviewScoreValue { get; set; }
        public decimal TaskScore { get; set; }
    }
}
