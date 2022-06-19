using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Api.Options
{
    public class DatabaseOptions
    {
        public const int DefaultTimeoutInSeconds = 30;

        [Required]
        public string ConnectionString { get; set; } = null!;

        public int TimeoutInSeconds { get; set; } = DefaultTimeoutInSeconds;

        /// <summary>
        /// 0 means that EnableRetryOnFailure disabled.
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Set false if Azure Identity and Azure Access Token is not used
        /// (for example when using raw connection string).
        /// </summary>
        public bool UseAzureAccessToken { get; set; } = true;
    }
}
