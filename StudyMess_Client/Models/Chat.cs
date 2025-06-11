namespace StudyMess_Client.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsGroupChat { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
