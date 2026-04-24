using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Common;
using SecondHandMarketplaceAPI.DTOs.Products;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Models.Enums;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ProductResponseDto>> GetAllAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Location.ToLower().Contains(keyword));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Condition) &&
                Enum.TryParse<ProductCondition>(filter.Condition.Replace(" ", ""), true, out var parsedCondition))
            {
                query = query.Where(p => p.Condition == parsedCondition);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            query = filter.SortBy?.ToLower() switch
            {
                "priceasc" or "price-low-high" => query.OrderBy(p => p.Price),
                "pricedesc" or "price-high-low" => query.OrderByDescending(p => p.Price),
                "available" or "availablefirst" => query.OrderByDescending(p => p.IsAvailable).ThenByDescending(p => p.CreatedAt),
                "sold" or "soldfirst" => query.OrderBy(p => p.IsAvailable).ThenByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 9 : filter.PageSize;

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                items.Add(await MapToResponseDtoAsync(product));
            }

            return new PagedResultDto<ProductResponseDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : await MapToResponseDtoAsync(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetMyProductsAsync(string sellerId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                result.Add(await MapToResponseDtoAsync(product));
            }

            return result;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetBySellerIdAsync(string sellerId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                result.Add(await MapToResponseDtoAsync(product));
            }

            return result;
        }

        public async Task<ProductResponseDto> CreateAsync(string sellerId, CreateProductDto dto)
        {
            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Condition = dto.Condition,
                CategoryId = dto.CategoryId,
                SellerId = sellerId,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var imageUrl in dto.ImageUrls.Distinct())
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = imageUrl,
                    BlobName = string.Empty
                });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var created = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstAsync(p => p.Id == product.Id);

            return await MapToResponseDtoAsync(created);
        }

        public async Task<ProductResponseDto?> UpdateAsync(int id, string sellerId, UpdateProductDto dto, bool isAdmin = false)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return null;
            }

            if (!isAdmin && product.SellerId != sellerId)
            {
                return null;
            }

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Location = dto.Location;
            product.Condition = dto.Condition;
            product.CategoryId = dto.CategoryId;
            product.IsAvailable = dto.IsAvailable;

            _context.ProductImages.RemoveRange(product.Images);
            product.Images.Clear();

            foreach (var imageUrl in dto.ImageUrls.Distinct())
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = imageUrl,
                    BlobName = string.Empty,
                    ProductId = product.Id
                });
            }

            await _context.SaveChangesAsync();

            var updated = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstAsync(p => p.Id == product.Id);

            return await MapToResponseDtoAsync(updated);
        }

        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin = false)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false;
            }

            if (!isAdmin && product.SellerId != userId)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<ProductResponseDto> MapToResponseDtoAsync(Product product)
        {
            var reviews = await _context.SellerReviews
                .Where(r => r.SellerId == product.SellerId)
                .ToListAsync();

            var totalReviews = reviews.Count;
            var averageRating = totalReviews == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1);

            var completedCount = await _context.Transactions.CountAsync(t =>
                t.SellerId == product.SellerId &&
                t.Status == "Completed");

            return new ProductResponseDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Location = product.Location,
                Condition = product.Condition,
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                SellerId = product.SellerId,
                SellerName = product.Seller?.FullName ?? string.Empty,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
                SellerAverageRating = averageRating,
                SellerTotalReviews = totalReviews,
                SellerVerified = completedCount >= 3
            };
        }
    }
}