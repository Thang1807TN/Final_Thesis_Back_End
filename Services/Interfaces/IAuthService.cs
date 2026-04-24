using SecondHandMarketplaceAPI.DTOs.Auth;
using SecondHandMarketplaceAPI.DTOs.Users;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<UserProfileDto?> GetCurrentUserAsync(string userId);
    }
}