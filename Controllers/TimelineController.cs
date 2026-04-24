using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimelineController : ControllerBase
    {
        private readonly IOrderTimelineService _orderTimelineService;

        public TimelineController(IOrderTimelineService orderTimelineService)
        {
            _orderTimelineService = orderTimelineService;
        }

        [HttpGet("transaction/{transactionId:int}")]
        public async Task<IActionResult> GetByTransactionId(int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var items = await _orderTimelineService.GetByTransactionIdAsync(transactionId, userId, isAdmin);
            return Ok(items);
        }
    }
}