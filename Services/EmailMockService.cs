using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class EmailMockService : IEmailMockService
    {
        private readonly ILogger<EmailMockService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailMockService(
            ILogger<EmailMockService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            _logger.LogInformation("===== MOCK EMAIL NOTIFICATION =====");
            _logger.LogInformation("To: {To}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);
            _logger.LogInformation("===================================");

            var log = new EmailNotificationLog
            {
                ToEmail = to,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
                Type = "Mock"
            };

            _context.EmailNotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}