using SecondHandMarketplaceAPI.Models;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}