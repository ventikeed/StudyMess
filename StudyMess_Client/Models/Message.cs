﻿namespace StudyMess_Client.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
    }
}
