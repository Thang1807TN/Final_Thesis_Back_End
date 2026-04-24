using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Admin;
using SecondHandMarketplaceAPI.Models.Enums;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class AdminAnalyticsService : IAdminAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AdminAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardAnalyticsDto> GetAnalyticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            var transactions = await _context.Transactions.ToListAsync();
            var reports = await _context.Reports.ToListAsync();
            var payments = await _context.Payments.ToListAsync();

            var usersByMonth = users
                .GroupBy(u => $"{u.CreatedAt.Year}-{u.CreatedAt.Month:D2}")
                .OrderBy(g => g.Key)
                .Select(g => new SimpleChartItemDto
                {
                    Name = g.Key,
                    Value = g.Count()
                })
                .ToList();

            var listingsByCategory = products
                .GroupBy(p => p.Category != null ? p.Category.Name : "Unknown")
                .Select(g => new SimpleChartItemDto
                {
                    Name = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            var transactionsByStatus = transactions
                .GroupBy(t => t.Status)
                .Select(g => new SimpleChartItemDto
                {
                    Name = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            var reportsByStatus = reports
                .GroupBy(r => r.Status)
                .Select(g => new SimpleChartItemDto
                {
                    Name = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            var revenueByMonth = payments
                .Where(p => p.PaymentStatus == PaymentStatus.Paid)
                .GroupBy(p => $"{p.CreatedAt.Year}-{p.CreatedAt.Month:D2}")
                .OrderBy(g => g.Key)
                .Select(g => new SimpleChartItemDto
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.Amount)
                })
                .ToList();

            return new AdminDashboardAnalyticsDto
            {
                UsersByMonth = usersByMonth,
                ListingsByCategory = listingsByCategory,
                TransactionsByStatus = transactionsByStatus,
                ReportsByStatus = reportsByStatus,
                RevenueByMonth = revenueByMonth
            };
        }
    }
}