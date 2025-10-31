namespace Bridgeon.Dtos
{
    public class ReviewScoreDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Week { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewerName { get; set; }
        public decimal AcademicScore { get; set; }
        public decimal ReviewScoreValue { get; set; }
        public decimal TaskScore { get; set; }
        public decimal TotalScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}