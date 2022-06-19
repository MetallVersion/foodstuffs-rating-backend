using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodstuffsRating.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FoodstuffsRating.Api.OAuth
{
    public interface IJwtTokenProvider
    {
        string CreateToken(Guid userId, string userEmail);
        ClaimsPrincipal ValidateToken(string jwtToken, bool validateLifetime = true);
    }

    public class JwtTokenProvider : IJwtTokenProvider
    {
        private readonly AuthOptions.JwtOptions _jwtOptions;
        private readonly ILogger<JwtTokenProvider> _logger;

        public JwtTokenProvider(IOptions<AuthOptions> authOptions,
            ILogger<JwtTokenProvider> logger)
        {
            this._logger = logger;
            this._jwtOptions = authOptions.Value.Jwt;
        }

        public string CreateToken(Guid userId, string userEmail)
        {
            this._logger.LogTrace("Start creating JWT token for userId: {userId}, userEmail: {userEmail}",
                userId, userEmail);

            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(this._jwtOptions.ExpirationInMinutes);

            var claims = CreateClaims(userId, userEmail);
            var subject = new ClaimsIdentity(claims);

            var key = this.CreateKey();
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JwtSecurityTokenHandler();

            var jwt = tokenHandler.CreateJwtSecurityToken(issuer: this._jwtOptions.Issuer,
                audience: this._jwtOptions.Audience,
                subject: subject,
                notBefore: issuedAt,
                expires: expiresAt,
                issuedAt: issuedAt,
                signingCredentials: signingCredentials);

            string jwtToken = tokenHandler.WriteToken(jwt);

            this._logger.LogTrace("JWT token created for userId: {userId}, userEmail: {userEmail}",
                userId, userEmail);

            return jwtToken;
        }

        public ClaimsPrincipal ValidateToken(string jwtToken, bool validateLifetime = true)
        {
            var key = this.CreateKey();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = this._jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = this._jwtOptions.Audience,
                RequireExpirationTime = true,
                ValidateLifetime = validateLifetime
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters,
                out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken)
            {
                this._logger.LogWarning("Provided token is not JWT");

                throw new SecurityTokenException("Token is not JWT");
            }

            if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                this._logger.LogWarning($"JWT signature algorithm is not expected, " +
                                        $"actual: {jwtSecurityToken.Header.Alg}");

                throw new SecurityTokenException("JWT signature algorithm is invalid");
            }

            return principal;
        }

        private SymmetricSecurityKey CreateKey()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._jwtOptions.IssuerSigningKey));

            return key;
        }

        private static Claim[] CreateClaims(Guid userId, string userEmail)
        {
            // TODO: add user roles into claims
            var claims = new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString("D")),
                new Claim(JwtRegisteredClaimNames.Email, userEmail),
                //new Claim(JwtRegisteredClaimNames.Name, userEmail),
                // asp net claims
                //new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // main claim in asp net - userId
                //new Claim(ClaimTypes.Name, userEmail), // name claim that used in asp net
            };

            return claims;
        }
    }
}
