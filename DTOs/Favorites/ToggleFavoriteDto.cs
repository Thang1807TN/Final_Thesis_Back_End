using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Favorites
{
    public class ToggleFavoriteDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}