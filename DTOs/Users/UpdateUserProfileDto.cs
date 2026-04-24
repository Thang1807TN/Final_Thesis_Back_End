using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Users
{
    public class UpdateUserProfileDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}