using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Reports;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportResponseDto?> CreateAsync(string userId, CreateReportDto dto)
        {
            if (!dto.ProductId.HasValue && string.IsNullOrWhiteSpace(dto.ReportedUserId))
            {
                return null;
            }

            var report = new Report
            {
                ReporterId = userId,
                ProductId = dto.ProductId,
                ReportedUserId = dto.ReportedUserId,
                Reason = dto.Reason.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var created = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Product)
                .Include(r => r.ReportedUser)
                .FirstAsync(r => r.Id == report.Id);

            return MapToResponseDto(created);
        }

        public async Task<IEnumerable<ReportResponseDto>> GetAllAsync()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Product)
                .Include(r => r.ReportedUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToResponseDto);
        }

        public async Task<ReportResponseDto?> UpdateStatusAsync(int id, UpdateReportStatusDto dto)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Product)
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return null;
            }

            report.Status = dto.Status.Trim();
            await _context.SaveChangesAsync();

            return MapToResponseDto(report);
        }

        private static ReportResponseDto MapToResponseDto(Report report)
        {
            return new ReportResponseDto
            {
                Id = report.Id,
                ReporterName = report.Reporter?.FullName ?? string.Empty,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
                ProductTitle = report.Product?.Title ?? string.Empty,
                ReportedUserName = report.ReportedUser?.FullName ?? string.Empty,
                CreatedAt = report.CreatedAt
            };
        }
    }
}