namespace SecondHandMarketplaceAPI.DTOs.Coupons
{
    public class CouponUsageStatsDto
    {
        public string Code { get; set; } = string.Empty;
        public int UsedCount { get; set; }
        public int UsageLimit { get; set; }
        public decimal UsageRatePercent { get; set; }
    }
}