using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Timeline;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class OrderTimelineService : IOrderTimelineService
    {
        private readonly ApplicationDbContext _context;

        public OrderTimelineService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderTimelineEventResponseDto?> AddAsync(int transactionId, string title, string description)
        {
            var transactionExists = await _context.Transactions.AnyAsync(t => t.Id == transactionId);
            if (!transactionExists)
            {
                return null;
            }

            var entity = new OrderTimelineEvent
            {
                TransactionId = transactionId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderTimelineEvents.Add(entity);
            await _context.SaveChangesAsync();

            return new OrderTimelineEventResponseDto
            {
                Id = entity.Id,
                TransactionId = entity.TransactionId,
                Title = entity.Title,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<IEnumerable<OrderTimelineEventResponseDto>> GetByTransactionIdAsync(int transactionId, string userId, bool isAdmin = false)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
            if (transaction == null)
            {
                return Enumerable.Empty<OrderTimelineEventResponseDto>();
            }

            if (!isAdmin && transaction.BuyerId != userId && transaction.SellerId != userId)
            {
                return Enumerable.Empty<OrderTimelineEventResponseDto>();
            }

            var items = await _context.OrderTimelineEvents
                .Where(x => x.TransactionId == transactionId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return items.Select(x => new OrderTimelineEventResponseDto
            {
                Id = x.Id,
                TransactionId = x.TransactionId,
                Title = x.Title,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            });
        }
    }
}