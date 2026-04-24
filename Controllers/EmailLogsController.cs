using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailLogsController : ControllerBase
    {
        private readonly IEmailLogService _emailLogService;

        public EmailLogsController(IEmailLogService emailLogService)
        {
            _emailLogService = emailLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _emailLogService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var bytes = await _emailLogService.ExportCsvAsync();
            return File(bytes, "text/csv", "email-logs.csv");
        }
    }
}