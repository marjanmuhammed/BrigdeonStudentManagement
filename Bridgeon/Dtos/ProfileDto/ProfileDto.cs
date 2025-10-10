using System;
using System.ComponentModel.DataAnnotations;

namespace Bridgeon.DTOs
{
    public class ProfileDto
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Phone, StringLength(15)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(100)]
        public string Branch { get; set; }

        [StringLength(100)]
        public string Space { get; set; }

        [Range(0, 52)]
        public int Week { get; set; }

        [StringLength(100)]
        public string Advisor { get; set; }

        [StringLength(100)]
        public string Mentor { get; set; }

        [StringLength(100)]
        public string Qualification { get; set; }

        [StringLength(200)]
        public string Institution { get; set; }

        [Range(1900, 2100)]
        public int PassOutYear { get; set; }

        [StringLength(100)]
        public string GuardianName { get; set; }

        [StringLength(50)]
        public string GuardianRelationship { get; set; }

        [Phone, StringLength(15)]
        public string GuardianPhone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}