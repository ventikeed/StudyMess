namespace StudyMess_Client.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? SenderUsername { get; set; }
        public string? Text { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentFileUrl { get; set; }
        public bool IsEdited { get; set; }


    }
}