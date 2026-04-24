using SecondHandMarketplaceAPI.DTOs.Seller;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface ISellerDashboardService
    {
        Task<SellerPublicProfileDto?> GetPublicProfileAsync(string sellerId);
    }
}