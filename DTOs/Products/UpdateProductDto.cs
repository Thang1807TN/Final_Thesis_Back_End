using System.ComponentModel.DataAnnotations;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Products
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999)]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public ProductCondition Condition { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public List<string> ImageUrls { get; set; } = new();
    }
}