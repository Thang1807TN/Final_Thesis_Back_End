using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Coupons
{
    public class UpdateCouponDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(1, 100)]
        public decimal DiscountPercent { get; set; }

        public bool IsActive { get; set; } = true;
        public int UsageLimit { get; set; } = 0;
        public DateTime? ExpiresAt { get; set; }
    }
}