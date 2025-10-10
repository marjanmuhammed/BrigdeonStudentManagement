namespace Bridgeon.Dtos.Mentor
{
    public class MentorAssignDto
    {
        public int MentorId { get; set; }
        public List<int> UserIds { get; set; } = new();
    }
}
