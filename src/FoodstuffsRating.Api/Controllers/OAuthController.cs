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

        public OAuthController(IUserManager userManager,
            IBackendRepository<User> userRepository,
            IBackendRepository<UserRefreshToken> refreshTokenRepository,
            IJwtTokenProvider jwtTokenProvider)
        {
            this._userManager = userManager;
            this._userRepository = userRepository;
            this._refreshTokenRepository = refreshTokenRepository;
            this._jwtTokenProvider = jwtTokenProvider;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token([FromForm(Name = "grant_type")] string grantType,
            [FromForm(Name = "username")] string userName,
            [FromForm(Name = "password")] string password,
            [FromForm(Name = "refresh_token")] string refreshToken
            //,[FromForm(Name = "scope")] string? scope // NOTE: not used yet
            )
        {
            if (grantType == "password")
            {
                return await this.PasswordCredentialsGrantAsync(userName, password);
            }
            if (grantType == "refresh_token")
            {
                return await this.RefreshTokenAsync(refreshToken);
            }

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
                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            var claimsPrincipal = this._jwtTokenProvider.ValidateToken(jwtToken, validateLifetime: false);
            var userId = claimsPrincipal.Claims.GetUserId();
            if (!userId.HasValue)
            {
                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            var user = await this._userRepository.GetAsync(x => x.Id == userId);
            if (user == null)
            {
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
                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            if (!existingRefreshToken.IsActive)
            {
                // TODO: we need to revoke all existing refresh tokens for user here
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

            var response = await this._userManager.IssueNewTokenAsync(user.Id);

            return this.Ok(response);
        }
        
        private async Task<IActionResult> PasswordCredentialsGrantAsync(string userName, string password)
        {
            var user = await this._userManager.GetUserByPassword(userName, password);
            if (user == null)
            {
                return this.BadRequest(new TokenErrorResponse
                {
                    Error = "invalid_grant"
                });
            }

            var response = await this._userManager.IssueNewTokenAsync(user.Id);

            return this.Ok(response);
        }
    }
}
