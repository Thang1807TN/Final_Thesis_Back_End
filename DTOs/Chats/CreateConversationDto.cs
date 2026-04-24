using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Chats
{
    public class CreateConversationDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}