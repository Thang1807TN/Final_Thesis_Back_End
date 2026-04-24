namespace SecondHandMarketplaceAPI.DTOs.Products
{
    public class ProductFilterDto
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? Condition { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "latest";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
}