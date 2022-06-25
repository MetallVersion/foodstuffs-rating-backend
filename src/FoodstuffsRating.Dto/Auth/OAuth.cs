using System.Text.Json.Serialization;
using FoodstuffsRating.Common.Constants;

namespace FoodstuffsRating.Dto
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// Access token expiration in seconds
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token expiration in seconds
        /// </summary>
        [JsonPropertyName("refresh_token_expires_in")]
        public int? RefreshTokenExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = AuthConstants.JwtBearer;

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    public class TokenErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = null!;

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }

        [JsonPropertyName("error_uri")]
        public string? ErrorUri { get; set; }
    }
}
