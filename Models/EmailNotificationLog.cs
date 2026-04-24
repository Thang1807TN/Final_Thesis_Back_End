namespace SecondHandMarketplaceAPI.Models
{
    public class EmailNotificationLog
    {
        public int Id { get; set; }

        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "Mock";
    }
}