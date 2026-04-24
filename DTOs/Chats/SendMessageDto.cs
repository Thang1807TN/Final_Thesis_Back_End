using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Chats
{
    public class SendMessageDto
    {
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public string? AttachmentName { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}