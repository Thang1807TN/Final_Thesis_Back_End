using System.Text;
using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Emails;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class EmailLogService : IEmailLogService
    {
        private readonly ApplicationDbContext _context;

        public EmailLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmailNotificationLogResponseDto>> GetAllAsync()
        {
            var logs = await _context.EmailNotificationLogs
                .OrderByDescending(x => x.SentAt)
                .ToListAsync();

            return logs.Select(x => new EmailNotificationLogResponseDto
            {
                Id = x.Id,
                ToEmail = x.ToEmail,
                Subject = x.Subject,
                Body = x.Body,
                Type = x.Type,
                SentAt = x.SentAt
            });
        }

        public async Task<byte[]> ExportCsvAsync()
        {
            var logs = await _context.EmailNotificationLogs
                .OrderByDescending(x => x.SentAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,ToEmail,Subject,Body,Type,SentAt");

            foreach (var item in logs)
            {
                sb.AppendLine(
                    $"{item.Id}," +
                    $"{Escape(item.ToEmail)}," +
                    $"{Escape(item.Subject)}," +
                    $"{Escape(item.Body)}," +
                    $"{Escape(item.Type)}," +
                    $"{Escape(item.SentAt.ToString("O"))}"
                );
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "\"\"";
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}