using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class BlobService : IBlobService
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var result = new List<string>();

            var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
            var containerName = _configuration["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
            {
                foreach (var file in files)
                {
                    result.Add($"https://placehold.co/600x400?text={Uri.EscapeDataString(file.FileName)}");
                }
                return result;
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var blobName = $"{Guid.NewGuid():N}{extension}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                result.Add(blobClient.Uri.ToString());
            }

            return result;
        }
    }
}