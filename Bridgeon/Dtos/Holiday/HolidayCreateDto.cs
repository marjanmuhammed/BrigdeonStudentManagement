using System.ComponentModel.DataAnnotations;

namespace Bridgeon.Dtos
{
    public class HolidayCreateDto
    {
        [Required]
        public DateTime Date { get; set; } // expect yyyy-MM-dd or ISO

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
