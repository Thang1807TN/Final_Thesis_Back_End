using System.ComponentModel.DataAnnotations;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.DTOs.Transactions
{
    public class UpdateTransactionStatusDto
    {
        [Required]
        public TransactionStatus Status { get; set; }
    }
}