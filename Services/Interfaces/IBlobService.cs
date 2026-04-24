using Microsoft.AspNetCore.Http;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IBlobService
    {
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);
    }
}