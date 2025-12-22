using System;

namespace QuizGeograficzny.Models
{
    public class ChatMessage
    {
        public string UserName { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}