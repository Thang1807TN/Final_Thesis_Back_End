using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Coupons;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CouponResponseDto>> GetAllAsync()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return coupons.Select(MapToResponseDto);
        }

        public async Task<CouponResponseDto?> GetByIdAsync(int id)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id);
            return coupon == null ? null : MapToResponseDto(coupon);
        }

        public async Task<CouponResponseDto?> CreateAsync(CreateCouponDto dto)
        {
            var exists = await _context.Coupons.AnyAsync(c => c.Code == dto.Code.Trim());
            if (exists)
            {
                return null;
            }

            var coupon = new Coupon
            {
                Code = dto.Code.Trim().ToUpper(),
                DiscountPercent = dto.DiscountPercent,
                IsActive = dto.IsActive,
                UsageLimit = dto.UsageLimit,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return MapToResponseDto(coupon);
        }

        public async Task<CouponResponseDto?> UpdateAsync(int id, UpdateCouponDto dto)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return null;
            }

            coupon.Code = dto.Code.Trim().ToUpper();
            coupon.DiscountPercent = dto.DiscountPercent;
            coupon.IsActive = dto.IsActive;
            coupon.UsageLimit = dto.UsageLimit;
            coupon.ExpiresAt = dto.ExpiresAt;

            await _context.SaveChangesAsync();
            return MapToResponseDto(coupon);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return false;
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CouponUsageStatsDto>> GetUsageStatsAsync()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.UsedCount)
                .ToListAsync();

            return coupons.Select(c => new CouponUsageStatsDto
            {
                Code = c.Code,
                UsedCount = c.UsedCount,
                UsageLimit = c.UsageLimit,
                UsageRatePercent = c.UsageLimit <= 0 ? 0 : Math.Round(c.UsedCount * 100m / c.UsageLimit, 2)
            });
        }

        private static CouponResponseDto MapToResponseDto(Coupon coupon)
        {
            return new CouponResponseDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountPercent = coupon.DiscountPercent,
                IsActive = coupon.IsActive,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                ExpiresAt = coupon.ExpiresAt,
                CreatedAt = coupon.CreatedAt
            };
        }
    }
}