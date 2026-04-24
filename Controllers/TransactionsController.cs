using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.DTOs.Transactions;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var items = await _transactionService.GetAllAsync(isAdmin, userId);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var item = await _transactionService.GetByIdAsync(id, isAdmin, userId);

            if (item == null)
            {
                return NotFound(new { message = "Transaction not found." });
            }

            return Ok(item);
        }

        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var items = await _transactionService.GetMineAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var created = await _transactionService.CreateAsync(buyerId, dto);

            if (created == null)
            {
                return BadRequest(new { message = "Could not create transaction." });
            }

            return Ok(created);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTransactionStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var updated = await _transactionService.UpdateStatusAsync(id, userId, dto, isAdmin);

            if (updated == null)
            {
                return BadRequest(new { message = "Could not update transaction status." });
            }

            return Ok(updated);
        }

        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var result = await _transactionService.MarkCompletedAsync(id, userId, isAdmin);

            if (result == null)
            {
                return BadRequest(new { message = "Could not mark transaction as completed." });
            }

            return Ok(result);
        }
    }
}