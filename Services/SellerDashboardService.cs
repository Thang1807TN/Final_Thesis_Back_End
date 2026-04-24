using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Seller;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class SellerDashboardService : ISellerDashboardService
    {
        private readonly ApplicationDbContext _context;

        public SellerDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SellerPublicProfileDto?> GetPublicProfileAsync(string sellerId)
        {
            var seller = await _context.Users.FirstOrDefaultAsync(u => u.Id == sellerId);
            if (seller == null)
            {
                return null;
            }

            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            var reviews = await _context.SellerReviews
                .Where(r => r.SellerId == sellerId)
                .ToListAsync();

            var completedTransactions = await _context.Transactions.CountAsync(t =>
                t.SellerId == sellerId && t.Status == "Completed");

            var totalListings = products.Count;
            var availableListings = products.Count(p => p.IsAvailable);
            var soldListings = products.Count(p => !p.IsAvailable);

            var totalReviews = reviews.Count;
            var averageRating = totalReviews == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1);

            return new SellerPublicProfileDto
            {
                SellerId = seller.Id,
                FullName = seller.FullName,
                JoinedAt = seller.CreatedAt,
                TotalListings = totalListings,
                AvailableListings = availableListings,
                SoldListings = soldListings,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                IsVerifiedSeller = completedTransactions >= 3
            };
        }
    }
}