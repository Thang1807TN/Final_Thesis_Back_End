using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Payments
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        public string? ExternalTransactionCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public string ProductTitle { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;

        public decimal OriginalTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? AppliedCouponCode { get; set; }
    }
}