using SecondHandMarketplaceAPI.DTOs.Reviews;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IReviewService
    {
        Task<SellerReviewResponseDto?> CreateAsync(string userId, CreateSellerReviewDto dto);
        Task<IEnumerable<SellerReviewResponseDto>> GetSellerReviewsAsync(string sellerId);
        Task<SellerRatingSummaryDto> GetSellerRatingSummaryAsync(string sellerId);
        Task<SellerReviewResponseDto?> MarkHelpfulAsync(int reviewId, ReviewHelpfulDto dto);
        Task<SellerReviewResponseDto?> ReplyAsync(int reviewId, string sellerId, SellerReplyDto dto);
    }
}