using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Reviews
{
    public class SellerReplyDto
    {
        [Required]
        [MaxLength(1000)]
        public string Reply { get; set; } = string.Empty;
    }
}