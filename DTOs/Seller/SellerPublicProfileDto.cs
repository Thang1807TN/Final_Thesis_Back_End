namespace SecondHandMarketplaceAPI.DTOs.Seller
{
    public class SellerPublicProfileDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }

        public int TotalListings { get; set; }
        public int AvailableListings { get; set; }
        public int SoldListings { get; set; }

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public bool IsVerifiedSeller { get; set; }
    }
}