using SecondHandMarketplaceAPI.DTOs.Emails;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IEmailLogService
    {
        Task<IEnumerable<EmailNotificationLogResponseDto>> GetAllAsync();
        Task<byte[]> ExportCsvAsync();
    }
}