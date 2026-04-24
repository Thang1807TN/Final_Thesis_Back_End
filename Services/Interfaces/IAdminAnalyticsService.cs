using SecondHandMarketplaceAPI.DTOs.Admin;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IAdminAnalyticsService
    {
        Task<AdminDashboardAnalyticsDto> GetAnalyticsAsync();
    }
}