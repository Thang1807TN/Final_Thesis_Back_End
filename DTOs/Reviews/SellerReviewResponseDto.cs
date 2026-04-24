namespace SecondHandMarketplaceAPI.DTOs.Reviews
{
    public class SellerReviewResponseDto
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int HelpfulCount { get; set; }
        public string? SellerReply { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}