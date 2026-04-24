using System.ComponentModel.DataAnnotations.Schema;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string BuyerId { get; set; } = string.Empty;
        public ApplicationUser? Buyer { get; set; }

        public string SellerId { get; set; } = string.Empty;
        public ApplicationUser? Seller { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Payment? Payment { get; set; }
        public ICollection<OrderTimelineEvent> TimelineEvents { get; set; } = new List<OrderTimelineEvent>();
    }
}