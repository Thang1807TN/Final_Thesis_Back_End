using SecondHandMarketplaceAPI.DTOs.Favorites;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> ToggleAsync(string userId, ToggleFavoriteDto dto);
        Task<IEnumerable<FavoriteResponseDto>> GetMineAsync(string userId);
        Task<bool> IsFavoriteAsync(string userId, int productId);
    }
}