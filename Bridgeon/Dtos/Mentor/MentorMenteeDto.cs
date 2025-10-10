namespace Bridgeon.Dtos.Mentor
{
    public class MentorMenteeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }

        public int? MentorId { get; set; }

        public string? MentorName { get; set; }
    }
}
