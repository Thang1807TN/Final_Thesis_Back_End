using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Payments
{
    public class CouponValidationDto
    {
        [Required]
        public int TransactionId { get; set; }

        [Required]
        public string CouponCode { get; set; } = string.Empty;
    }
}