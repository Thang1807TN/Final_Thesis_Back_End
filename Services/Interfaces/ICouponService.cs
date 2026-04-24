using SecondHandMarketplaceAPI.DTOs.Coupons;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponResponseDto>> GetAllAsync();
        Task<CouponResponseDto?> GetByIdAsync(int id);
        Task<CouponResponseDto?> CreateAsync(CreateCouponDto dto);
        Task<CouponResponseDto?> UpdateAsync(int id, UpdateCouponDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CouponUsageStatsDto>> GetUsageStatsAsync();
    }
}