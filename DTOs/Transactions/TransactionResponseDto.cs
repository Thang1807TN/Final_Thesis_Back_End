namespace SecondHandMarketplaceAPI.DTOs.Transactions
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;

        public string BuyerId { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;

        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;

        // 👉 Giá gốc
        public decimal TotalAmount { get; set; }

        // 👉 Giá sau giảm
        public decimal? PaidAmount { get; set; }

        // 👉 Số tiền giảm
        public decimal DiscountAmount { get; set; }

        // 👉 % giảm
        public int DiscountPercent { get; set; }

        // 👉 Coupon đã dùng
        public string? AppliedCouponCode { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int? PaymentId { get; set; }
        public string? PaymentStatus { get; set; }
        public bool IsPaid { get; set; }
    }
}