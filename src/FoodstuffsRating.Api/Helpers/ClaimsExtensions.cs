using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodstuffsRating.Api.Helpers
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Gets user Id from identity claims.
        /// </summary>
        /// <param name="claims">Claims collection</param>
        public static Guid? GetUserId(this IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            var claimsList = claims.ToList();
            var claim = claimsList.FirstOrDefault(a => string.Equals(a.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))
                        ?? claimsList.FirstOrDefault(a => string.Equals(a.Type, JwtRegisteredClaimNames.Sub, StringComparison.OrdinalIgnoreCase));
            var userIdRaw = claim?.Value;
            if (userIdRaw != null &&
                Guid.TryParse(userIdRaw, out Guid userId))
            {
                return userId;
            }

            return null;
        }
    }
}
