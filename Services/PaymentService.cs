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

                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == validation.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsedCount += 1;
                    }
                }
            }

            var finalAmount = Math.Max(0, originalTotal - discountAmount);

            if (transaction.Payment != null)
            {
                var existingPayment = await _context.Payments
                    .Include(p => p.Transaction)
                        .ThenInclude(t => t.Product)
                    .Include(p => p.Transaction)
                        .ThenInclude(t => t.Buyer)
                    .Include(p => p.Transaction)
                        .ThenInclude(t => t.Seller)
                    .FirstOrDefaultAsync(p => p.Id == transaction.Payment.Id);

                if (existingPayment == null)
                {
                    return null;
                }

                existingPayment.PaymentMethod = dto.PaymentMethod;
                existingPayment.Amount = finalAmount;

                ApplyPaymentStatusToTransaction(
                    existingPayment,
                    dto.PaymentMethod == PaymentMethod.CashOnDelivery
                        ? PaymentStatus.Pending
                        : PaymentStatus.Paid
                );

                if (existingPayment.PaymentStatus == PaymentStatus.Paid)
                {
                    existingPayment.ExternalTransactionCode = dto.PaymentMethod == PaymentMethod.OnlineDemo
                        ? $"DEMO-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
                        : dto.PaymentMethod == PaymentMethod.BankTransfer
                            ? $"BANK-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
                            : existingPayment.ExternalTransactionCode;
                }

                await _context.SaveChangesAsync();

                await _orderTimelineService.AddAsync(
                    existingPayment.TransactionId,
                    "Payment confirmed",
                    $"Payment method: {existingPayment.PaymentMethod}. Status: {existingPayment.PaymentStatus}."
                );

                return MapToResponseDto(
                    existingPayment,
                    existingPayment.Transaction?.TotalAmount ?? existingPayment.Amount,
                    discountAmount,
                    appliedCouponCode
                );
            }

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
            else
            {
                transaction.Status = "Pending";
            }

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                transaction.Id,
                "Payment created",
                $"Method: {payment.PaymentMethod}, Status: {payment.PaymentStatus}"
            );

            await SendPaymentCreatedEmailsAsync(transaction, payment);

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
                    p.Transaction != null &&
                    (p.Transaction.BuyerId == userId || p.Transaction.SellerId == userId));
            }

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(p =>
                MapToResponseDto(p, p.Transaction?.TotalAmount ?? p.Amount, 0, null));
        }

        public async Task<PaymentResponseDto?> UpdateStatusAsync(
            int id,
            UpdatePaymentStatusDto dto,
            string userId,
            bool isAdmin = false)
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

            var isOwner = payment.Transaction?.BuyerId == userId ||
                          payment.Transaction?.SellerId == userId;

            if (!isAdmin && !isOwner)
            {
                return null;
            }

            if (!isAdmin && dto.PaymentStatus == PaymentStatus.Refunded)
            {
                return null;
            }

            ApplyPaymentStatusToTransaction(payment, dto.PaymentStatus);

            await _context.SaveChangesAsync();

            await _orderTimelineService.AddAsync(
                payment.TransactionId,
                "Payment updated",
                $"Status: {payment.PaymentStatus}. Transaction status: {payment.Transaction?.Status ?? "N/A"}."
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

        private static void ApplyPaymentStatusToTransaction(Payment payment, PaymentStatus status)
        {
            payment.PaymentStatus = status;

            if (payment.Transaction == null)
            {
                return;
            }

            switch (status)
            {
                case PaymentStatus.Paid:
                    payment.PaidAt = DateTime.UtcNow;
                    payment.Transaction.Status = "Paid";

                    if (payment.Transaction.Product != null)
                    {
                        payment.Transaction.Product.IsAvailable = false;
                    }

                    break;

                case PaymentStatus.Pending:
                    payment.PaidAt = null;
                    payment.Transaction.Status = "Pending";

                    if (payment.Transaction.Product != null)
                    {
                        payment.Transaction.Product.IsAvailable = true;
                    }

                    break;

                case PaymentStatus.Failed:
                    payment.PaidAt = null;
                    payment.Transaction.Status = "Pending";

                    if (payment.Transaction.Product != null)
                    {
                        payment.Transaction.Product.IsAvailable = true;
                    }

                    break;

                case PaymentStatus.Refunded:
                    payment.PaidAt = null;
                    payment.Transaction.Status = "Cancelled";

                    if (payment.Transaction.Product != null)
                    {
                        payment.Transaction.Product.IsAvailable = true;
                    }

                    break;
            }
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
                result.Message = "Enter coupon code.";
                return result;
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c =>
                c.Code == result.CouponCode &&
                c.IsActive &&
                (!c.ExpiresAt.HasValue || c.ExpiresAt > DateTime.UtcNow) &&
                (c.UsageLimit == 0 || c.UsedCount < c.UsageLimit));

            if (coupon == null)
            {
                result.Message = "Invalid or expired coupon.";
                return result;
            }

            result.IsValid = true;
            result.DiscountPercent = coupon.DiscountPercent;
            result.DiscountAmount = Math.Round(transaction.TotalAmount * coupon.DiscountPercent / 100m, 2);
            result.FinalTotal = Math.Max(0, transaction.TotalAmount - result.DiscountAmount);
            result.Message = $"Coupon applied successfully. Discount: {coupon.DiscountPercent}%";

            return result;
        }

        private async Task SendPaymentCreatedEmailsAsync(Transaction transaction, Payment payment)
        {
            if (!string.IsNullOrWhiteSpace(transaction.Buyer?.Email))
            {
                await _emailMockService.SendAsync(
                    transaction.Buyer.Email,
                    "Payment created",
                    "Payment has been created."
                );
            }

            if (!string.IsNullOrWhiteSpace(transaction.Seller?.Email))
            {
                await _emailMockService.SendAsync(
                    transaction.Seller.Email,
                    "Payment recorded",
                    "Payment has been recorded."
                );
            }
        }

        private static PaymentResponseDto MapToResponseDto(
            Payment payment,
            decimal originalTotal,
            decimal discountAmount,
            string? couponCode)
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