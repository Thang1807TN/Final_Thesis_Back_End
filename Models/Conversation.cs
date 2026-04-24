namespace SecondHandMarketplaceAPI.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string BuyerId { get; set; } = string.Empty;
        public ApplicationUser? Buyer { get; set; }

        public string SellerId { get; set; } = string.Empty;
        public ApplicationUser? Seller { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}