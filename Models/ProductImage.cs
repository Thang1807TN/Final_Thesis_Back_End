using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string BlobName { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}