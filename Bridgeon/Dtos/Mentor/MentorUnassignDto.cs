namespace Bridgeon.Dtos.Mentor
{
    public class MentorUnassignDto
    {
        public int MentorId { get; set; }
        public List<int> UserIds { get; set; } = new();
    }

}
