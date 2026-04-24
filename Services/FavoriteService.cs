using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Favorites;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleAsync(string userId, ToggleFavoriteDto dto)
        {
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == dto.ProductId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId);
            if (!productExists)
            {
                return false;
            }

            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                ProductId = dto.ProductId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FavoriteResponseDto>> GetMineAsync(string userId)
        {
            var favorites = await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p!.Images)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return favorites.Select(f => new FavoriteResponseDto
            {
                Id = f.Id,
                ProductId = f.ProductId,
                ProductTitle = f.Product?.Title ?? string.Empty,
                Price = f.Product?.Price ?? 0,
                Location = f.Product?.Location ?? string.Empty,
                IsAvailable = f.Product?.IsAvailable ?? false,
                ImageUrl = f.Product?.Images.FirstOrDefault()?.ImageUrl,
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<bool> IsFavoriteAsync(string userId, int productId)
        {
            return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        }
    }
}