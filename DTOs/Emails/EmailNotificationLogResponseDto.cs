namespace SecondHandMarketplaceAPI.DTOs.Emails
{
    public class EmailNotificationLogResponseDto
    {
        public int Id { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}