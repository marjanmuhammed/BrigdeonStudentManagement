namespace Bridgeon.Dtos
{
    public class UpdateReviewScoreDto
    {
        public int Week { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewerName { get; set; }
        public decimal AcademicScore { get; set; }
        public decimal ReviewScoreValue { get; set; }
        public decimal TaskScore { get; set; }
    }
}
