using FoodstuffsRating.Api.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Api.Controllers
{
    [Route("oauth/external")]
    public class OAuthExternalLoginController : ControllerBase
    {
        private readonly GoogleAuthOptions _googleOptions;

        public OAuthExternalLoginController(IOptions<GoogleAuthOptions> googleOptions)
        {
            this._googleOptions = googleOptions.Value;
        }

        // [authSchema = Google]
        [HttpGet]
        [Route("google")]
        public async Task<IActionResult> Google([FromQuery] string code,
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

            return this.Ok(tokenResponse.IdToken);
        }
    }
}
