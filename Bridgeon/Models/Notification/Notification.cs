
using System;

namespace Bridgeon.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "review", "fee", "system", etc.
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public User User { get; set; }

        public string GetFormattedDate()
        {
            return CreatedAt.ToString("MMMM dd, yyyy h:mm tt");
        }
    }
}