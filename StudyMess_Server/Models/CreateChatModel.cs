namespace StudyMess_Server.Models
{
    public class CreateChatModel
    {
        public string Name { get; set; } = null!;
        public bool IsGroupChat { get; set; }
        public string? Avatar { get; set; }
        public List<int> UserIds { get; set; } = new();
    }
}
