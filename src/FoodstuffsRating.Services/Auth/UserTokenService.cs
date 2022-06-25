using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodstuffsRating.Common.Constants;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Models.Exceptions;
using FoodstuffsRating.Services.Helpers;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Services.Auth
{
    public interface IUserTokenService
    {
        Task<TokenResponse> IssueNewTokenAsync(Guid userId);
        Task<TokenResponse> RefreshTokenAsync(string accessToken,
            string refreshToken);
    }

    public class UserTokenService : IUserTokenService
    {
        private readonly IBackendRepository<User> _userRepository;

        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        private readonly ILogger<UserTokenService> _logger;

        public UserTokenService(IBackendRepository<User> userRepository,
            IAccessTokenService accessTokenService,
            IRefreshTokenService refreshTokenService,
            ILogger<UserTokenService> logger)
        {
            this._userRepository = userRepository;
            this._accessTokenService = accessTokenService;
            this._refreshTokenService = refreshTokenService;
            this._logger = logger;
        }
        
        public async Task<TokenResponse> IssueNewTokenAsync(Guid userId)
        {
            var user = await this._userRepository.GetAsync(x => x.Id == userId, asNoTracking: false);
            if (user == null)
            {
                this._logger.LogWarning("User was not found by provided Id: {userId}", userId);

                throw new ApiException("User was not found");
            }

            var accessToken = this._accessTokenService.IssueNewAccessToken(userId, user.Email);
            var refreshToken = await this._refreshTokenService.IssueNewRefreshTokenAsync(userId);

            user.LastLoginDateUtc = DateTime.UtcNow;
            await this._userRepository.UpdateAsync(user);

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken.Token,
                ExpiresIn = accessToken.ExpiresIn,

                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresIn = refreshToken.ExpiresIn,
            };

            return tokenResponse;
        }

        public async Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var claimsPrincipal = this.ValidateExpiredAccessToken(accessToken);

            var userIdRaw = claimsPrincipal.GetUserId();
            if (!Guid.TryParse(userIdRaw, out Guid userId))
            {
                this._logger.LogWarning("JWT token claims does not contain userId");

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }

            // TODO: check that user finished registration

            var existingRefreshToken = await this._refreshTokenService.GetRefreshTokenAsync(
                refreshToken, userId);

            using var ls = this._logger.BeginScope("{userId} {refreshTokenId}",
                userId, existingRefreshToken.Id);

            if (!existingRefreshToken.IsActive)
            {
                this._logger.LogError("Refresh token exists, but it is inactive, " +
                    "revoke all existing refresh tokens!");

                await this._refreshTokenService.RevokeAllRefreshTokensAsync(userId);

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }

            await this._refreshTokenService.RevokeRefreshTokenAsync(existingRefreshToken.Id);
            this._logger.LogTrace("Existing token revoked");

            var model = await this.IssueNewTokenAsync(userId);

            return model;
        }
 
        private ClaimsPrincipal ValidateExpiredAccessToken(string accessToken)
        {
            try
            {
                return this._accessTokenService.ValidateExpiredAccessToken(accessToken);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "JWT token is not valid");

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }
        }
    }
}
