using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondHandMarketplaceAPI.DTOs.Coupons;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _couponService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _couponService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound(new { message = "Coupon not found." });
            }

            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            var item = await _couponService.CreateAsync(dto);

            if (item == null)
            {
                return BadRequest(new { message = "Could not create coupon." });
            }

            return Ok(item);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponDto dto)
        {
            var item = await _couponService.UpdateAsync(id, dto);

            if (item == null)
            {
                return NotFound(new { message = "Coupon not found." });
            }

            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _couponService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Coupon not found." });
            }

            return Ok(new { message = "Coupon deleted successfully." });
        }

        [HttpGet("usage-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsageStats()
        {
            var items = await _couponService.GetUsageStatsAsync();
            return Ok(items);
        }
    }
}