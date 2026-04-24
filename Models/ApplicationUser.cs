using Microsoft.AspNetCore.Identity;

namespace SecondHandMarketplaceAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Transaction> BuyerTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> SellerTransactions { get; set; } = new List<Transaction>();
    }
}