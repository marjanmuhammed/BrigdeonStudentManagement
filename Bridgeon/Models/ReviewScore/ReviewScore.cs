using Bridgeon.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ReviewScore
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int Week { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ReviewerName { get; set; }

    [Range(0, 100)]
    public decimal AcademicScore { get; set; }

    [Range(0, 100)]
    public decimal ReviewScoreValue { get; set; }

    [Range(0, 100)]
    public decimal TaskScore { get; set; }

    // Make this a stored database column
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }

    // Optional: Keep computed property for convenience
    [NotMapped]
    public decimal ComputedTotalScore => AcademicScore + ReviewScoreValue + TaskScore;
}