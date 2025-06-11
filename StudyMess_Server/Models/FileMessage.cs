namespace StudyMess_Server.Models
{
    public class FileMessage
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public IFormFile? File { get; set; }
        public string filepath { get; set; } = null!;
    }
}