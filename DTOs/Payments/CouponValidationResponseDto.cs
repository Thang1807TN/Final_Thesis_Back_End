namespace SecondHandMarketplaceAPI.DTOs.Payments
{
    public class CouponValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;

        public decimal OriginalTotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }

        public string CouponCode { get; set; } = string.Empty;
    }
}