namespace StudyMess_Client.Models
{
    public class ChatMember
    {
        public int Id { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.Now;
        public string Role { get; set; } = null!;
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
