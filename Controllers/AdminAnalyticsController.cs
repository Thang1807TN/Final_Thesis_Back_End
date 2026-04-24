using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IAdminAnalyticsService _adminAnalyticsService;

        public AdminAnalyticsController(IAdminAnalyticsService adminAnalyticsService)
        {
            _adminAnalyticsService = adminAnalyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _adminAnalyticsService.GetAnalyticsAsync();
            return Ok(data);
        }
    }
}