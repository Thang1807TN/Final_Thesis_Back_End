using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Reviews;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SellerReviewResponseDto?> CreateAsync(string userId, CreateSellerReviewDto dto)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .FirstOrDefaultAsync(t => t.Id == dto.TransactionId);

            if (transaction == null || transaction.BuyerId != userId)
            {
                return null;
            }

            if (!string.Equals(transaction.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var exists = await _context.SellerReviews.AnyAsync(r => r.TransactionId == dto.TransactionId);
            if (exists)
            {
                return null;
            }

            var review = new SellerReview
            {
                TransactionId = transaction.Id,
                ReviewerId = userId,
                SellerId = transaction.SellerId,
                Rating = dto.Rating,
                Comment = dto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.SellerReviews.Add(review);
            await _context.SaveChangesAsync();

            var created = await _context.SellerReviews
                .Include(r => r.Reviewer)
                .FirstAsync(r => r.Id == review.Id);

            return MapToResponseDto(created);
        }

        public async Task<IEnumerable<SellerReviewResponseDto>> GetSellerReviewsAsync(string sellerId)
        {
            var reviews = await _context.SellerReviews
                .Include(r => r.Reviewer)
                .Where(r => r.SellerId == sellerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToResponseDto);
        }

        public async Task<SellerRatingSummaryDto> GetSellerRatingSummaryAsync(string sellerId)
        {
            var query = _context.SellerReviews.Where(r => r.SellerId == sellerId);

            var totalReviews = await query.CountAsync();
            var avg = totalReviews == 0 ? 0 : await query.AverageAsync(r => r.Rating);

            return new SellerRatingSummaryDto
            {
                SellerId = sellerId,
                AverageRating = Math.Round(avg, 1),
                TotalReviews = totalReviews
            };
        }

        public async Task<SellerReviewResponseDto?> MarkHelpfulAsync(int reviewId, ReviewHelpfulDto dto)
        {
            var review = await _context.SellerReviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
            {
                return null;
            }

            if (dto.Increment)
            {
                review.HelpfulCount += 1;
            }
            else if (review.HelpfulCount > 0)
            {
                review.HelpfulCount -= 1;
            }

            await _context.SaveChangesAsync();
            return MapToResponseDto(review);
        }

        public async Task<SellerReviewResponseDto?> ReplyAsync(int reviewId, string sellerId, SellerReplyDto dto)
        {
            var review = await _context.SellerReviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null || review.SellerId != sellerId)
            {
                return null;
            }

            review.SellerReply = dto.Reply.Trim();
            await _context.SaveChangesAsync();

            return MapToResponseDto(review);
        }

        private static SellerReviewResponseDto MapToResponseDto(SellerReview review)
        {
            return new SellerReviewResponseDto
            {
                Id = review.Id,
                TransactionId = review.TransactionId,
                ReviewerName = review.Reviewer?.FullName ?? string.Empty,
                SellerId = review.SellerId,
                Rating = review.Rating,
                Comment = review.Comment,
                HelpfulCount = review.HelpfulCount,
                SellerReply = review.SellerReply,
                CreatedAt = review.CreatedAt
            };
        }
    }
}