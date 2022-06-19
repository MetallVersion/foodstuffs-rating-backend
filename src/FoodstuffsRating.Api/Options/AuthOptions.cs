using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Api.Options
{
    public class AuthOptions
    {
        [Required]
        public JwtOptions Jwt { get; set; } = null!;

        [Required]
        public RefreshTokenOptions RefreshToken { get; set; } = null!;

        public class JwtOptions
        {
            [Required]
            public string IssuerSigningKey { get; set; } = null!;

            [Required]
            public string Issuer { get; set; } = null!;

            [Required]
            public string Audience { get; set; } = null!;

            [Required]
            public int ExpirationInMinutes { get; set; }
        }

        public class RefreshTokenOptions
        {
            [Required]
            public int ExpirationInMinutes { get; set; }
        }
    }
}
