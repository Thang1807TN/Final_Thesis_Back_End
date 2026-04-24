using SecondHandMarketplaceAPI.DTOs.Common;
using SecondHandMarketplaceAPI.DTOs.Products;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductResponseDto>> GetAllAsync(ProductFilterDto filter);
        Task<ProductResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductResponseDto>> GetMyProductsAsync(string sellerId);
        Task<IEnumerable<ProductResponseDto>> GetBySellerIdAsync(string sellerId);
        Task<ProductResponseDto> CreateAsync(string sellerId, CreateProductDto dto);
        Task<ProductResponseDto?> UpdateAsync(int id, string sellerId, UpdateProductDto dto, bool isAdmin = false);
        Task<bool> DeleteAsync(int id, string userId, bool isAdmin = false);
    }
}