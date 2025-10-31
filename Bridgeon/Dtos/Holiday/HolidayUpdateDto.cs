using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos
{
    public class HolidayUpdateDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
