using SecondHandMarketplaceAPI.DTOs.Timeline;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IOrderTimelineService
    {
        Task<OrderTimelineEventResponseDto?> AddAsync(int transactionId, string title, string description);
        Task<IEnumerable<OrderTimelineEventResponseDto>> GetByTransactionIdAsync(int transactionId, string userId, bool isAdmin = false);
    }
}