using System;
using System.Linq;
using System.Threading.Tasks;
using FoodstuffsRating.Common.Constants;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route($"{ApiPrefix.Url}/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IUserTokenService _userTokenService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(IUserTokenService userTokenService,
            IUserRegistrationService userRegistrationService,
            ILogger<OAuthController> logger)
        {
            this._userTokenService = userTokenService;
            this._userRegistrationService = userRegistrationService;
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
            using var ls = this._logger.BeginScope("{grant_type} {username}", grantType, username);

            if (grantType == OAuthConstants.GrantTypes.PasswordCredentials)
            {
                return await this.PasswordCredentialsGrantAsync(username, password);
            }
            if (grantType == OAuthConstants.GrantTypes.RefreshToken)
            {
                this._logger.LogTrace($"Refresh Token grant type, refresh token: {refreshToken}");

                return await this.RefreshTokenAsync(refreshToken);
            }

            this._logger.LogWarning($"Unsupported grant type: {grantType}");

            return this.BadRequest(new TokenErrorResponse
            {
                Error = OAuthConstants.Errors.UnsupportedGrantType
            });
        }

        private async Task<IActionResult> PasswordCredentialsGrantAsync(string username, string password)
        {
            var user = await this._userRegistrationService.GetUserByPassword(username, password);
            if (user == null)
            {
                this._logger.LogInformation("Username or password is not valid");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = OAuthConstants.Errors.InvalidGrant
                });
            }

            var response = await this._userTokenService.IssueNewTokenAsync(user.Id);

            this._logger.LogTrace("New token issued");

            return this.Ok(response);
        }

        private async Task<IActionResult> RefreshTokenAsync(string refreshToken)
        {
            string authorizationHeader = this.Request.Headers.Authorization.ToString();
            string? accessToken = authorizationHeader.Split(' ').ElementAtOrDefault(1);
            if (accessToken == null)
            {
                this._logger.LogWarning("JWT token was not provided with request");
                this._logger.LogTrace($"Authorization header value: {authorizationHeader}");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = OAuthConstants.Errors.InvalidGrant
                });
            }

            try
            {
                var tokenResponse = await this._userTokenService.RefreshTokenAsync(accessToken, refreshToken);

                return this.Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error occurred during refreshing token");
            }

            return this.BadRequest(new TokenErrorResponse
            {
                Error = OAuthConstants.Errors.InvalidGrant
            });
        }
    }
}
