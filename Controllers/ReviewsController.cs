using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.DTOs.Reviews;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateSellerReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _reviewService.CreateAsync(userId, dto);

            if (result == null)
            {
                return BadRequest(new { message = "Could not create review." });
            }

            return Ok(result);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetSellerReviews(string sellerId)
        {
            var items = await _reviewService.GetSellerReviewsAsync(sellerId);
            return Ok(items);
        }

        [HttpGet("seller/{sellerId}/summary")]
        public async Task<IActionResult> GetSellerSummary(string sellerId)
        {
            var summary = await _reviewService.GetSellerRatingSummaryAsync(sellerId);
            return Ok(summary);
        }

        [HttpPut("{reviewId:int}/helpful")]
        [Authorize]
        public async Task<IActionResult> MarkHelpful(int reviewId, [FromBody] ReviewHelpfulDto dto)
        {
            var result = await _reviewService.MarkHelpfulAsync(reviewId, dto);

            if (result == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            return Ok(result);
        }

        [HttpPut("{reviewId:int}/reply")]
        [Authorize]
        public async Task<IActionResult> Reply(int reviewId, [FromBody] SellerReplyDto dto)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _reviewService.ReplyAsync(reviewId, sellerId, dto);

            if (result == null)
            {
                return BadRequest(new { message = "Could not reply to review." });
            }

            return Ok(result);
        }
    }
}