using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.DTOs.Products;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            var products = await _productService.GetAllAsync(filter);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(product);
        }

        [HttpGet("my-listings")]
        [Authorize]
        public async Task<IActionResult> GetMyListings()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var products = await _productService.GetMyProductsAsync(sellerId);
            return Ok(products);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetBySellerId(string sellerId)
        {
            var items = await _productService.GetBySellerIdAsync(sellerId);
            return Ok(items);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var created = await _productService.CreateAsync(sellerId, dto);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var updated = await _productService.UpdateAsync(id, userId, dto, isAdmin);

            if (updated == null)
            {
                return NotFound(new { message = "Product not found or access denied." });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var deleted = await _productService.DeleteAsync(id, userId, isAdmin);

            if (!deleted)
            {
                return NotFound(new { message = "Product not found or access denied." });
            }

            return Ok(new { message = "Product listing deleted successfully. Transactions and payments are kept." });
        }
    }
}