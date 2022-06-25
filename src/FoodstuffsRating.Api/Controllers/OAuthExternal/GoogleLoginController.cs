using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FoodstuffsRating.Common.Constants;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Models.Auth;
using FoodstuffsRating.Models.Options;
using FoodstuffsRating.Services.Auth;
using FoodstuffsRating.Services.AuthExternal;
using FoodstuffsRating.Services.Helpers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Api.Controllers
{
    [Authorize(AuthenticationSchemes = AuthConstants.GoogleAuthScheme)]
    [Route($"{ApiPrefix.Url}/oauth/google")]
    public class GoogleLoginController : ControllerBase
    {
        private const ExternalLoginProvider LoginProvider = ExternalLoginProvider.Google;

        private readonly IExternalProviderRegistrationService _externalRegistrationService;
        private readonly IUserTokenService _userTokenService;
        private readonly IMapper _mapper;
        private readonly GoogleAuthOptions _googleOptions;
        private readonly ILogger<GoogleLoginController> _logger;

        public GoogleLoginController(
            IExternalProviderRegistrationService externalRegistrationService,
            IUserTokenService userTokenService,
            IMapper mapper,
            IOptions<GoogleAuthOptions> googleOptions,
            ILogger<GoogleLoginController> logger)
        {
            this._externalRegistrationService = externalRegistrationService;
            this._userTokenService = userTokenService;
            this._mapper = mapper;
            this._googleOptions = googleOptions.Value;
            this._logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetGoogleToken([FromQuery] string code,
            [FromQuery] string state, [FromQuery] string scope,
            CancellationToken cancellationToken)
        {
            var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = this._googleOptions.ClientId,
                    ClientSecret = this._googleOptions.ClientSecret
                },
                DataStore = null
            });

            var tokenResponse = await codeFlow.ExchangeCodeForTokenAsync(null, code,
                this._googleOptions.RedirectUrl, cancellationToken);

            var jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(tokenResponse?.IdToken))
            {
                var jwt = jwtHandler.ReadJwtToken(tokenResponse!.IdToken);

                return this.Ok(new
                {
                    id_token = tokenResponse.IdToken,
                    claims = jwt.Claims.ToDictionary(x => x.Type, x => x.Value)
                });
            }

            this._logger.LogWarning("Oauth token received from Google is not valid!");

            return this.BadRequest();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterFromGoogle(
            [FromBody] UserRegistrationFromExternalRequest registrationRequest)
        {
            string? externalUserId = this.User.GetUserId();
            string? userEmail = this.User.GetUserEmail();

            if (!this.IsGoogleClaimsValid(externalUserId, userEmail, out var errorMessage))
            {
                this.BadRequest(errorMessage);
            }

            var model = this._mapper.Map<ExternalProviderRegistrationModel>(registrationRequest);
            model.LoginProvider = LoginProvider;
            model.Email = userEmail!;
            model.ExternalUserId = externalUserId!;

            using var ls = this._logger.BeginScope("{externalUserId}, {userEmail}, {loginProvider}",
                model.ExternalUserId, model.Email, model.LoginProvider);

            await this._externalRegistrationService.RegisterAsync(model);

            return this.NoContent();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginViaGoogle()
        {
            string? externalUserId = this.User.GetUserId();
            string? userEmail = this.User.GetUserEmail();

            if (!this.IsGoogleClaimsValid(externalUserId, userEmail, out var errorMessage))
            {
                this.BadRequest(errorMessage);
            }

            using var ls = this._logger.BeginScope("{externalUserId}, {userEmail}, {loginProvider}",
                externalUserId, userEmail, LoginProvider);

            var user = await this._externalRegistrationService.GetUserByExternalLoginAsync(
                externalUserId!, userEmail!, LoginProvider);
            if (user == null)
            {
                this._logger.LogInformation("User login via Google failed");

                return this.BadRequest(new TokenErrorResponse
                {
                    Error = OAuthConstants.Errors.InvalidGrant
                });
            }

            var response = await this._userTokenService.IssueNewTokenAsync(user.Id);

            this._logger.LogTrace("New token issued for external Google login");

            return this.Ok(response);
        }

        private bool IsGoogleClaimsValid(string? externalUserId, string? userEmail, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                errorMessage = $"Claim '{JwtRegisteredClaimNames.Sub}' is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                errorMessage = $"Claim '{JwtRegisteredClaimNames.Email}' is required";
                return false;
            }

            errorMessage = null!;
            return true;
        }
    }
}
