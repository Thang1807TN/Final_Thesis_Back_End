using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Reviews
{
    public class CreateSellerReviewDto
    {
        [Required]
        public int TransactionId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}