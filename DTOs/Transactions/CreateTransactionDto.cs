using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Transactions
{
    public class CreateTransactionDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}