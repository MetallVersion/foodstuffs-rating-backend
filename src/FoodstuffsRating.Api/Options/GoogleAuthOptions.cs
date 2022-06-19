using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Api.Options
{
    public class GoogleAuthOptions
    {
        [Required]
        public string ClientId { get; set; } = null!;

        [Required]
        public string ClientSecret { get; set; } = null!;

        [Required]
        public string RedirectUrl { get; set; } = null!;

        [Required]
        public string Authority { get; set; } = null!;

        [Required]
        public string OpenIdConfigurationUrl { get; set; } = null!;
    }
}
