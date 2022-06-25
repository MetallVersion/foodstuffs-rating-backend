using System;
using System.Threading.Tasks;
using FoodstuffsRating.Common.Constants;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Models.Auth;
using FoodstuffsRating.Models.Exceptions;
using FoodstuffsRating.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<TokenModel> IssueNewRefreshTokenAsync(Guid userId);
        Task<UserRefreshToken> GetRefreshTokenAsync(string refreshToken, Guid userId);
        Task RevokeRefreshTokenAsync(long id);
        Task RevokeAllRefreshTokensAsync(Guid userId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IBackendRepository<UserRefreshToken> _refreshTokenRepository;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            IBackendRepository<UserRefreshToken> refreshTokenRepository,
            IRefreshTokenProvider refreshTokenProvider,
            IOptions<AuthOptions> authOptions,
            ILogger<RefreshTokenService> logger)
        {
            this._refreshTokenRepository = refreshTokenRepository;
            this._refreshTokenProvider = refreshTokenProvider;
            this._logger = logger;
            this._authOptions = authOptions.Value;
        }

        public async Task<TokenModel> IssueNewRefreshTokenAsync(Guid userId)
        {
            int refreshTokenExpiresIn = (int)TimeSpan.FromMinutes(
                this._authOptions.RefreshToken.ExpirationInMinutes).TotalSeconds;
            string refreshToken = this._refreshTokenProvider.GenerateRefreshToken();

            var userRefreshToken = new UserRefreshToken
            {
                RefreshToken = refreshToken,
                IsActive = true,
                UserId = userId,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(
                    this._authOptions.RefreshToken.ExpirationInMinutes)
            };
            await this._refreshTokenRepository.AddAsync(userRefreshToken);

            this._logger.LogTrace($"New refresh token successfully issued, for userId: {userId}");

            return new TokenModel
            {
                Token = refreshToken,
                ExpiresIn = refreshTokenExpiresIn
            };
        }

        public async Task<UserRefreshToken> GetRefreshTokenAsync(string refreshToken, Guid userId)
        {
            var existingRefreshToken = await this._refreshTokenRepository.GetAsync(
                x => x.RefreshToken == refreshToken, asNoTracking: true, x => x.User);
            if (existingRefreshToken == null)
            {
                this._logger.LogWarning("Refresh token was not found, {refreshToken}", refreshToken);

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            if (existingRefreshToken.User?.Id != userId)
            {
                this._logger.LogWarning(
                    $"Refresh token belongs to another user Id: {existingRefreshToken.User?.Id}");

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }

            return existingRefreshToken;
        }

        public async Task RevokeRefreshTokenAsync(long id)
        {
            using var ls = this._logger.BeginScope("{refreshTokenId}", id);

            var refreshToken = await this._refreshTokenRepository.GetAsync(x => x.Id == id,
                asNoTracking: false);
            if (refreshToken == null)
            {
                this._logger.LogWarning("Refresh token not found");

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }
            if (!refreshToken.IsActive)
            {
                this._logger.LogWarning("Refresh token already revoked");

                throw new OAuthTokenException(OAuthConstants.Errors.InvalidGrant);
            }

            refreshToken.IsActive = false;
            await this._refreshTokenRepository.UpdateAsync(refreshToken);

            this._logger.LogInformation("Refresh token successfully revoked");
        }

        public async Task RevokeAllRefreshTokensAsync(Guid userId)
        {
            var refreshTokens = await this._refreshTokenRepository.GetManyAsync(
                x => x.UserId == userId && x.IsActive, asNoTracking: false);

            foreach (var refreshToken in refreshTokens)
            {
                this._logger.LogInformation($"Refresh token revoked, id: {refreshToken.Id}");

                refreshToken.IsActive = false;
                await this._refreshTokenRepository.UpdateAsync(refreshToken, commitChanges: false);
            }

            await this._refreshTokenRepository.SaveChangesAsync();

            this._logger.LogInformation($"All active refresh tokens revoked, for userId: {userId}",
                userId);
        }
    }
}