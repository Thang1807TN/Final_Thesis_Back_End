using Microsoft.AspNetCore.Mvc;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerDashboardController : ControllerBase
    {
        private readonly ISellerDashboardService _sellerDashboardService;

        public SellerDashboardController(ISellerDashboardService sellerDashboardService)
        {
            _sellerDashboardService = sellerDashboardService;
        }

        [HttpGet("public/{sellerId}")]
        public async Task<IActionResult> GetPublicProfile(string sellerId)
        {
            var profile = await _sellerDashboardService.GetPublicProfileAsync(sellerId);

            if (profile == null)
            {
                return NotFound(new { message = "Seller not found." });
            }

            return Ok(profile);
        }
    }
}