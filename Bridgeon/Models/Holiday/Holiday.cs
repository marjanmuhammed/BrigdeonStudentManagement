using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        // store date as DateOnly (if using .NET 6+); otherwise use DateTime (with Date part)
        [Required]
        public DateTime Date { get; set; } // use only the date portion

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        // optional: created/updated metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
