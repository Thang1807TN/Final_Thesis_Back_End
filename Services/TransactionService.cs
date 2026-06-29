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

            var transaction = new Transaction
            {
                ProductId = product.Id,
                BuyerId = buyerId,
                SellerId = product.SellerId,
                TotalAmount = product.Price,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Transaction created",
                $"Transaction created for \"{product.Title}\"."
            );

            return MapToResponseDto(transaction);
        }

        public async Task<TransactionResponseDto?> UpdateStatusAsync(
            int id,
            string userId,
            UpdateTransactionStatusDto dto,
            bool isAdmin)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
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

            switch (dto.Status)
            {
                case TransactionStatus.Pending:
                    transaction.Status = "Pending";

                    if (transaction.Payment != null)
                    {
                        transaction.Payment.PaymentStatus = PaymentStatus.Pending;
                        transaction.Payment.PaidAt = null;
                    }

                    if (transaction.Product != null)
                    {
                        transaction.Product.IsAvailable = true;
                    }
                    break;

                case TransactionStatus.Paid:
                    transaction.Status = "Paid";

                    if (transaction.Payment != null)
                    {
                        transaction.Payment.PaymentStatus = PaymentStatus.Paid;
                        transaction.Payment.PaidAt = DateTime.UtcNow;
                    }

                    if (transaction.Product != null)
                    {
                        transaction.Product.IsAvailable = false;
                    }
                    break;

                case TransactionStatus.Completed:
                    if (!string.Equals(transaction.Status, "Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    transaction.Status = "Completed";
                    break;

                case TransactionStatus.Cancelled:
                    transaction.Status = "Cancelled";

                    if (transaction.Payment != null)
                    {
                        transaction.Payment.PaymentStatus = PaymentStatus.Failed;
                        transaction.Payment.PaidAt = null;
                    }

                    if (transaction.Product != null)
                    {
                        transaction.Product.IsAvailable = true;
                    }
                    break;

                default:
                    transaction.Status = dto.Status.ToString();
                    break;
            }

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Transaction updated",
                $"Status changed to {transaction.Status}"
            );

            return MapToResponseDto(transaction);
        }

        public async Task<TransactionResponseDto?> MarkCompletedAsync(
            int id,
            string userId,
            bool isAdmin = false)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
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
                "Transaction marked as completed"
            );

            return MapToResponseDto(transaction);
        }

        private static TransactionResponseDto MapToResponseDto(Transaction transaction)
        {
            var originalAmount = transaction.TotalAmount;
            var paidAmount = transaction.Payment?.Amount;

            var discountAmount = paidAmount.HasValue
                ? Math.Max(0, originalAmount - paidAmount.Value)
                : 0;

            var discountPercent =
                originalAmount > 0 && discountAmount > 0
                    ? (int)Math.Round((discountAmount / originalAmount) * 100)
                    : 0;

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductTitle = transaction.Product?.Title ?? "",
                BuyerId = transaction.BuyerId,
                BuyerName = transaction.Buyer?.FullName ?? "",
                SellerId = transaction.SellerId,
                SellerName = transaction.Seller?.FullName ?? "",
                TotalAmount = originalAmount,
                PaidAmount = paidAmount,
                DiscountAmount = discountAmount,
                DiscountPercent = discountPercent,
                AppliedCouponCode = transaction.Payment?.AppliedCouponCode,
                Status = transaction.Status,
                CreatedAt = transaction.CreatedAt,
                PaymentId = transaction.Payment?.Id,
                PaymentStatus = transaction.Payment?.PaymentStatus.ToString(),
                IsPaid = transaction.Payment?.PaymentStatus == PaymentStatus.Paid
            };
        }
    }
}