namespace SecondHandMarketplaceAPI.DTOs.Chats
{
    public class ConversationResponseDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;

        public string BuyerId { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;

        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public string LastMessagePreview { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
    }
}