using System.ComponentModel.DataAnnotations;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Payments
{
    public class CreatePaymentDto
    {
        [Required]
        public int TransactionId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string? CouponCode { get; set; }
    }
}