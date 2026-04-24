using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Transactions;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Models.Enums;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderTimelineService _orderTimelineService;

        public TransactionService(
            ApplicationDbContext context,
            IOrderTimelineService orderTimelineService)
        {
            _context = context;
            _orderTimelineService = orderTimelineService;
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetAllAsync(bool isAdmin, string userId)
        {
            var query = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(t => t.BuyerId == userId || t.SellerId == userId);
            }

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToResponseDto);
        }

        public async Task<TransactionResponseDto?> GetByIdAsync(int id, bool isAdmin, string userId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return null;
            }

            if (!isAdmin && transaction.BuyerId != userId && transaction.SellerId != userId)
            {
                return null;
            }

            return MapToResponseDto(transaction);
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetMineAsync(string userId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .Where(t => t.BuyerId == userId || t.SellerId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToResponseDto);
        }

        public async Task<TransactionResponseDto?> CreateAsync(string buyerId, CreateTransactionDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null || !product.IsAvailable || product.SellerId == buyerId)
            {
                return null;
            }

            var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Id == buyerId);
            if (buyer == null)
            {
                return null;
            }

            var transaction = new Transaction
            {
                ProductId = product.Id,
                BuyerId = buyerId,
                SellerId = product.SellerId,
                TotalAmount = product.Price,
                Status = TransactionStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Transaction created",
                $"A transaction was created for product \"{product.Title}\"."
            );

            var created = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .FirstAsync(t => t.Id == transaction.Id);

            return MapToResponseDto(created);
        }

        public async Task<TransactionResponseDto?> UpdateStatusAsync(int id, string userId, UpdateTransactionStatusDto dto, bool isAdmin)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return null;
            }

            var isOwner = transaction.BuyerId == userId || transaction.SellerId == userId;
            if (!isAdmin && !isOwner)
            {
                return null;
            }

            transaction.Status = dto.Status.ToString();

            if (dto.Status == TransactionStatus.Completed && transaction.Product != null)
            {
                transaction.Product.IsAvailable = false;
            }

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Transaction status updated",
                $"Transaction status changed to {transaction.Status}."
            );

            return MapToResponseDto(transaction);
        }

        public async Task<TransactionResponseDto?> MarkCompletedAsync(int id, string userId, bool isAdmin = false)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return null;
            }

            if (!isAdmin && transaction.BuyerId != userId && transaction.SellerId != userId)
            {
                return null;
            }

            if (!string.Equals(transaction.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            transaction.Status = "Completed";
            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Order completed",
                "The transaction has been marked as completed."
            );

            return MapToResponseDto(transaction);
        }

        private static TransactionResponseDto MapToResponseDto(Transaction transaction)
        {
            return new TransactionResponseDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductTitle = transaction.Product?.Title ?? string.Empty,
                BuyerId = transaction.BuyerId,
                BuyerName = transaction.Buyer?.FullName ?? string.Empty,
                SellerId = transaction.SellerId,
                SellerName = transaction.Seller?.FullName ?? string.Empty,
                TotalAmount = transaction.TotalAmount,
                Status = transaction.Status,
                CreatedAt = transaction.CreatedAt,
                PaymentId = transaction.Payment?.Id,
                PaymentStatus = transaction.Payment?.PaymentStatus.ToString(),
                IsPaid = transaction.Payment?.PaymentStatus == PaymentStatus.Paid
            };
        }
    }
}