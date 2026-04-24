using SecondHandMarketplaceAPI.DTOs.Payments;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto?> CreateAsync(string userId, CreatePaymentDto dto);
        Task<PaymentResponseDto?> GetByIdAsync(int id, string userId, bool isAdmin = false);
        Task<IEnumerable<PaymentResponseDto>> GetMyPaymentsAsync(string userId, bool isAdmin = false);
        Task<PaymentResponseDto?> UpdateStatusAsync(int id, UpdatePaymentStatusDto dto, string userId, bool isAdmin = false);
        Task<CouponValidationResponseDto> ValidateCouponAsync(string userId, CouponValidationDto dto);
    }
}