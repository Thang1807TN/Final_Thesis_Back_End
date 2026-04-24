using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.DTOs.Payments;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentInvoiceService _paymentInvoiceService;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentInvoiceService paymentInvoiceService)
        {
            _paymentService = paymentService;
            _paymentInvoiceService = paymentInvoiceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var payment = await _paymentService.CreateAsync(userId, dto);

            if (payment == null)
            {
                return BadRequest(new { message = "Could not create payment." });
            }

            return Ok(payment);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var payment = await _paymentService.GetByIdAsync(id, userId, isAdmin);

            if (payment == null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Ok(payment);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var payments = await _paymentService.GetMyPaymentsAsync(userId, isAdmin);
            return Ok(payments);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var updated = await _paymentService.UpdateStatusAsync(id, dto, userId, isAdmin);

            if (updated == null)
            {
                return BadRequest(new { message = "Could not update payment status." });
            }

            return Ok(updated);
        }

        [HttpPost("validate-coupon")]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _paymentService.ValidateCouponAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet("{id:int}/invoice")]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var fileBytes = await _paymentInvoiceService.GenerateInvoicePdfAsync(id, userId, isAdmin);
            if (fileBytes == null)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            return File(fileBytes, "application/pdf", $"payment-invoice-{id}.pdf");
        }
    }
}