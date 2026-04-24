using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public ProductCondition Condition { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new();

        public double SellerAverageRating { get; set; }
        public int SellerTotalReviews { get; set; }
        public bool SellerVerified { get; set; }
    }
}