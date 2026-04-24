using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string? ExternalTransactionCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}