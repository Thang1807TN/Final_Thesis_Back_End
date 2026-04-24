namespace SecondHandMarketplaceAPI.DTOs.Reviews
{
    public class SellerRatingSummaryDto
    {
        public string SellerId { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}