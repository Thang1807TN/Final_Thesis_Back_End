using SecondHandMarketplaceAPI.DTOs.Reports;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportResponseDto?> CreateAsync(string userId, CreateReportDto dto);
        Task<IEnumerable<ReportResponseDto>> GetAllAsync();
        Task<ReportResponseDto?> UpdateStatusAsync(int id, UpdateReportStatusDto dto);
    }
}