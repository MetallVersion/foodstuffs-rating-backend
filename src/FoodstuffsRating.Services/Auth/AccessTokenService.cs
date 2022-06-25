using System;
using System.Security.Claims;
using FoodstuffsRating.Models.Auth;
using FoodstuffsRating.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Services.Auth
{
    public interface IAccessTokenService
    {
        TokenModel IssueNewAccessToken(Guid userId, string userEmail);
        ClaimsPrincipal ValidateExpiredAccessToken(string accessToken);
    }

    public class AccessTokenService : IAccessTokenService
    {
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<AccessTokenService> _logger;

        public AccessTokenService(IJwtTokenProvider jwtTokenProvider,
            IOptions<AuthOptions> authOptions,
            ILogger<AccessTokenService> logger)
        {
            this._authOptions = authOptions.Value;
            this._logger = logger;
            this._jwtTokenProvider = jwtTokenProvider;
        }

        public TokenModel IssueNewAccessToken(Guid userId, string userEmail)
        {
            using var ls = this._logger.BeginScope("{userId} {userEmail}", userId, userEmail);

            int expiresIn = (int)TimeSpan.FromMinutes(this._authOptions.Jwt.ExpirationInMinutes)
                .TotalSeconds;
            string jwtToken = this._jwtTokenProvider.CreateToken(userId, userEmail);


            var accessToken = new TokenModel
            {
                Token = jwtToken,
                ExpiresIn = expiresIn
            };

            return accessToken;
        }

        public ClaimsPrincipal ValidateExpiredAccessToken(string accessToken)
        {
            return this._jwtTokenProvider.ValidateToken(accessToken, validateLifetime: false);
        }
    }
}