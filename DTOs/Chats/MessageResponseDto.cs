namespace SecondHandMarketplaceAPI.DTOs.Chats
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string? AttachmentName { get; set; }
        public string? AttachmentUrl { get; set; }

        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}