using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Payments;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Models.Enums;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailMockService _emailMockService;
        private readonly IOrderTimelineService _orderTimelineService;

        public PaymentService(
            ApplicationDbContext context,
            IEmailMockService emailMockService,
            IOrderTimelineService orderTimelineService)
        {
            _context = context;
            _emailMockService = emailMockService;
            _orderTimelineService = orderTimelineService;
        }

        public async Task<PaymentResponseDto?> CreateAsync(string userId, CreatePaymentDto dto)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == dto.TransactionId);

            if (transaction == null || transaction.BuyerId != userId)
            {
                return null;
            }

            if (transaction.Payment != null)
            {
                return MapToResponseDto(transaction.Payment, transaction.TotalAmount, 0, null);
            }

            decimal originalTotal = transaction.TotalAmount;
            decimal discountAmount = 0;
            string? appliedCouponCode = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var validation = await ValidateCouponInternalAsync(transaction, dto.CouponCode);
                if (validation.IsValid)
                {
                    discountAmount = validation.DiscountAmount;
                    appliedCouponCode = validation.CouponCode;

                    var coupon = await _context.Coupons.FirstAsync(c => c.Code == validation.CouponCode);
                    coupon.UsedCount += 1;
                }
            }

            var finalAmount = originalTotal - discountAmount;
            if (finalAmount < 0) finalAmount = 0;

            var payment = new Payment
            {
                TransactionId = transaction.Id,
                Amount = finalAmount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.CashOnDelivery
                    ? PaymentStatus.Pending
                    : PaymentStatus.Paid,
                ExternalTransactionCode = dto.PaymentMethod == PaymentMethod.OnlineDemo
                    ? $"DEMO-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
                    : dto.PaymentMethod == PaymentMethod.BankTransfer
                        ? $"BANK-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
                        : null,
                PaidAt = dto.PaymentMethod == PaymentMethod.CashOnDelivery ? null : DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            if (payment.PaymentStatus == PaymentStatus.Paid)
            {
                transaction.Status = "Paid";
                if (transaction.Product != null)
                {
                    transaction.Product.IsAvailable = false;
                }
            }

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Payment created",
                $"Payment method: {payment.PaymentMethod}. Status: {payment.PaymentStatus}."
            );

            if (!string.IsNullOrWhiteSpace(appliedCouponCode))
            {
                await _orderTimelineService.AddAsync(
                    transaction.Id,
                    "Coupon applied",
                    $"Coupon {appliedCouponCode} applied. Discount amount: {discountAmount}."
                );
            }

            await _emailMockService.SendAsync(
                transaction.Buyer?.Email ?? string.Empty,
                $"Payment created for transaction #{transaction.Id}",
                $"Hello {transaction.Buyer?.FullName},\n\nYour payment for product \"{transaction.Product?.Title}\" has been created.\nMethod: {payment.PaymentMethod}\nStatus: {payment.PaymentStatus}\nAmount: {payment.Amount}\n\nThis is a mock email from GreenMarket."
            );

            await _emailMockService.SendAsync(
                transaction.Seller?.Email ?? string.Empty,
                "A payment has been recorded for your listing",
                $"Hello {transaction.Seller?.FullName},\n\nA payment related to your product \"{transaction.Product?.Title}\" has been recorded.\nBuyer: {transaction.Buyer?.FullName}\nStatus: {payment.PaymentStatus}\nAmount: {payment.Amount}\n\nThis is a mock email from GreenMarket."
            );

            var created = await _context.Payments
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Product)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Buyer)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Seller)
                .FirstAsync(p => p.Id == payment.Id);

            return MapToResponseDto(created, originalTotal, discountAmount, appliedCouponCode);
        }

        public async Task<PaymentResponseDto?> GetByIdAsync(int id, string userId, bool isAdmin = false)
        {
            var payment = await _context.Payments
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Product)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Buyer)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return null;
            }

            if (!isAdmin &&
                payment.Transaction?.BuyerId != userId &&
                payment.Transaction?.SellerId != userId)
            {
                return null;
            }

            return MapToResponseDto(payment, payment.Transaction?.TotalAmount ?? payment.Amount, 0, null);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetMyPaymentsAsync(string userId, bool isAdmin = false)
        {
            var query = _context.Payments
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Product)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Buyer)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Seller)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(p =>
                    p.Transaction!.BuyerId == userId ||
                    p.Transaction!.SellerId == userId);
            }

            var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return payments.Select(p => MapToResponseDto(p, p.Transaction?.TotalAmount ?? p.Amount, 0, null));
        }

        public async Task<PaymentResponseDto?> UpdateStatusAsync(int id, UpdatePaymentStatusDto dto, string userId, bool isAdmin = false)
        {
            var payment = await _context.Payments
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Product)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Buyer)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return null;
            }

            var isOwner = payment.Transaction?.BuyerId == userId || payment.Transaction?.SellerId == userId;
            if (!isAdmin && !isOwner)
            {
                return null;
            }

            payment.PaymentStatus = dto.PaymentStatus;

            if (dto.PaymentStatus == PaymentStatus.Paid)
            {
                payment.PaidAt = DateTime.UtcNow;

                if (payment.Transaction != null)
                {
                    payment.Transaction.Status = "Paid";

                    if (payment.Transaction.Product != null)
                    {
                        payment.Transaction.Product.IsAvailable = false;
                    }
                }

                await _emailMockService.SendAsync(
                    payment.Transaction?.Buyer?.Email ?? string.Empty,
                    $"Payment completed for transaction #{payment.TransactionId}",
                    $"Hello {payment.Transaction?.Buyer?.FullName},\n\nYour payment for \"{payment.Transaction?.Product?.Title}\" is now marked as Paid.\n\nThis is a mock email from GreenMarket."
                );

                await _emailMockService.SendAsync(
                    payment.Transaction?.Seller?.Email ?? string.Empty,
                    "Payment received for your product",
                    $"Hello {payment.Transaction?.Seller?.FullName},\n\nThe payment for your product \"{payment.Transaction?.Product?.Title}\" has been completed.\n\nThis is a mock email from GreenMarket."
                );
            }

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                payment.TransactionId,
                "Payment status updated",
                $"Payment status changed to {dto.PaymentStatus}."
            );

            return MapToResponseDto(payment, payment.Transaction?.TotalAmount ?? payment.Amount, 0, null);
        }

        public async Task<CouponValidationResponseDto> ValidateCouponAsync(string userId, CouponValidationDto dto)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == dto.TransactionId && t.BuyerId == userId);

            if (transaction == null)
            {
                return new CouponValidationResponseDto
                {
                    IsValid = false,
                    Message = "Transaction not found."
                };
            }

            return await ValidateCouponInternalAsync(transaction, dto.CouponCode);
        }

        private async Task<CouponValidationResponseDto> ValidateCouponInternalAsync(Transaction transaction, string couponCode)
        {
            var result = new CouponValidationResponseDto
            {
                CouponCode = couponCode?.Trim() ?? string.Empty,
                OriginalTotal = transaction.TotalAmount,
                FinalTotal = transaction.TotalAmount
            };

            if (string.IsNullOrWhiteSpace(result.CouponCode))
            {
                result.Message = "Please enter a coupon code.";
                return result;
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c =>
                c.Code == result.CouponCode &&
                c.IsActive &&
                (!c.ExpiresAt.HasValue || c.ExpiresAt > DateTime.UtcNow) &&
                (c.UsageLimit == 0 || c.UsedCount < c.UsageLimit));

            if (coupon == null)
            {
                result.Message = "Coupon is invalid or expired.";
                return result;
            }

            result.IsValid = true;
            result.DiscountPercent = coupon.DiscountPercent;
            result.DiscountAmount = Math.Round(transaction.TotalAmount * coupon.DiscountPercent / 100m, 2);
            result.FinalTotal = transaction.TotalAmount - result.DiscountAmount;
            if (result.FinalTotal < 0) result.FinalTotal = 0;
            result.Message = $"Coupon applied successfully. Discount: {coupon.DiscountPercent}%";

            return result;
        }

        private static PaymentResponseDto MapToResponseDto(Payment payment, decimal originalTotal, decimal discountAmount, string? couponCode)
        {
            return new PaymentResponseDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                ExternalTransactionCode = payment.ExternalTransactionCode,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                ProductTitle = payment.Transaction?.Product?.Title ?? string.Empty,
                BuyerName = payment.Transaction?.Buyer?.FullName ?? string.Empty,
                SellerName = payment.Transaction?.Seller?.FullName ?? string.Empty,
                OriginalTotal = originalTotal,
                DiscountAmount = discountAmount,
                AppliedCouponCode = couponCode
            };
        }
    }
}