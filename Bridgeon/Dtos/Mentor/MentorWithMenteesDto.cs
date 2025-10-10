namespace Bridgeon.Dtos.Mentor
{
    public class MentorWithMenteesDto
    {
        public int MentorId { get; set; }
        public string MentorName { get; set; }
        public List<MentorMenteeDto> Mentees { get; set; } = new();
    }
}
