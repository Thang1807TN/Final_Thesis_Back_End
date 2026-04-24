using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Timeline
{
    public class CreateTimelineEventDto
    {
        [Required]
        public int TransactionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}