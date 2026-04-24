using System.ComponentModel.DataAnnotations;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Payments
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public PaymentStatus PaymentStatus { get; set; }
    }
}