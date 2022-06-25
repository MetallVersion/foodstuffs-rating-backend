namespace FoodstuffsRating.Models.Auth
{
    public class TokenModel
    {
        public string Token { get; set; } = null!;

        /// <summary>
        /// Token expiration in seconds
        /// </summary>
        public int ExpiresIn { get; set; }
    }
}
