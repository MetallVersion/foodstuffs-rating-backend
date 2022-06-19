using System.Security.Claims;
using FoodstuffsRating.Api.Dto;
using FoodstuffsRating.Api.Helpers;
using FoodstuffsRating.Api.OAuth;
using FoodstuffsRating.Api.Services;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstuffsRating.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IBackendRepository<User> _userRepository;
        private readonly IBackendRepository<UserRefreshToken> _refreshTokenRepository;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(IUserManager userManager,
            IBackendRepository<User> userRepository,
            IBackendRepository<UserRefreshToken> refreshTokenRepository,
            IJwtTokenProvider jwtTokenProvider,
            ILogger<OAuthController> logger)
        {
            this._userManager = userManager;
            this._userRepository = userRepository;
            this._refreshTokenRepository = refreshTokenRepository;
            this._jwtTokenProvider = jwtTokenProvider;
            this._logger = logger;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token([FromForm(Name = "grant_type")] string grantType,
            [FromForm(Name = "username")] string username,
            [FromForm(Name = "password")] string password,
            [FromForm(Name = "refresh_token")] string refreshToken
            //,[FromForm(Name = "scope")] string? scope // NOTE: not used yet
            )
        {
            this._logger.BeginScope("{grant_type} {username}", grantType, username);

            if (grantType == "password")
            {
                return await this.PasswordCredentialsGrantAsync(username, password);
            }
            if (grantType == "refresh_token")
            {
                this._logger.LogTrace($"Refresh Token grant type, refresh token: {refreshToken}");

                return await this.RefreshTokenAsync(refreshToken);
            }

            this._logger.LogWarning("Unsupported grant type");

            return this.BadRequest(new TokenErrorResponse
            {
                Error = "unsupported_grant_type"
            });
        }

        private async Task<IActionResult> RefreshTokenAsync(string refreshToken)
        {
            string? jwtToken = this.Request.Headers.Authorization.ToString()
                .Split(' ').ElementAtOrDefault(1);
            if (jwtToken == null)
            {
                this._logger.LogWarning("JWT token was not provided with request");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            ClaimsPrincipal claimsPrincipal;
            try
            {
                claimsPrincipal = this._jwtTokenProvider.ValidateToken(jwtToken, validateLifetime: false);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "JWT token is not valid");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }
            var userId = claimsPrincipal.Claims.GetUserId();
            if (!userId.HasValue)
            {
                this._logger.LogWarning("JWT token claims does not contain userId");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            var user = await this._userRepository.GetAsync(x => x.Id == userId);
            if (user == null)
            {
                this._logger.LogWarning("User not found by userId: {userId}", userId);

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            // TODO: check that user finished registration
            
            var existingRefreshToken = await this._refreshTokenRepository.GetAsync(x => x.RefreshToken == refreshToken,
                asNoTracking: false);
            if (existingRefreshToken == null)
            {
                this._logger.LogWarning("Refresh token was not found");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            if (!existingRefreshToken.IsActive)
            {
                this._logger.LogError("Refresh token found, but is inactive, revoke all existing refresh tokens!");

                var userWithRefreshTokens = await this._userRepository.GetAsync(x => x.Id == userId,
                    asNoTracking: false, x => x.RefreshTokens);
                foreach (var oldRefreshToken in userWithRefreshTokens!.RefreshTokens)
                {
                    oldRefreshToken.IsActive = false;
                    await this._refreshTokenRepository.UpdateAsync(oldRefreshToken, commitChanges: false);
                }
                await this._refreshTokenRepository.SaveChangesAsync();

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            existingRefreshToken.IsActive = false;
            await this._refreshTokenRepository.UpdateAsync(existingRefreshToken);

            this._logger.LogTrace("Existing token revoked");

            var response = await this._userManager.IssueNewTokenAsync(user.Id);

            this._logger.LogTrace("New token issued");

            return this.Ok(response);
        }
        
        private async Task<IActionResult> PasswordCredentialsGrantAsync(string username, string password)
        {
            var user = await this._userManager.GetUserByPassword(username, password);
            if (user == null)
            {
                this._logger.LogInformation("Username or password is not valid");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            var response = await this._userManager.IssueNewTokenAsync(user.Id);

            this._logger.LogTrace("New token issued");

            return this.Ok(response);
        }
    }
}
