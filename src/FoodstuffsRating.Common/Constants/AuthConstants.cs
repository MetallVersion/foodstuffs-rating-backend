namespace FoodstuffsRating.Common.Constants
{
    public static class AuthConstants
    {
        public const string GoogleAuthScheme = "Google.Jwt";
        public const string JwtBearer = "Bearer";
    }

    public static class OAuthConstants
    {
        public static class Errors
        {
            public const string UnsupportedGrantType = "unsupported_grant_type";
            public const string InvalidGrant = "invalid_grant";
        }

        public static class GrantTypes
        {
            public const string PasswordCredentials = "password";
            public const string RefreshToken = "refresh_token";
        }
    }
    
}
