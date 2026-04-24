using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public UploadsController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost("images")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files uploaded." });
            }

            var uploadedUrls = await _blobService.UploadImagesAsync(files);
            return Ok(uploadedUrls);
        }
    }
}