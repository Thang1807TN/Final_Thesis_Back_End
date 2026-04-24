namespace SecondHandMarketplaceAPI.Models
{
    public class SellerReview
    {
        public int Id { get; set; }

        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }

        public string ReviewerId { get; set; } = string.Empty;
        public ApplicationUser? Reviewer { get; set; }

        public string SellerId { get; set; } = string.Empty;
        public ApplicationUser? Seller { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public int HelpfulCount { get; set; } = 0;
        public string? SellerReply { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}