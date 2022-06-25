using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FoodstuffsRating.Models.Exceptions;

namespace FoodstuffsRating.Services.Helpers
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Gets user Id as Guid from claims.
        /// It searches in next claims: 'sub', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'.
        /// </summary>
        public static Guid GetUserIdAsGuid(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ArgumentNullException(nameof(claimsPrincipal));

            string? userIdRaw = claimsPrincipal.GetUserId();

            if (userIdRaw != null &&
                Guid.TryParse(userIdRaw, out Guid userId))
            {
                return userId;
            }

            throw new UnauthorizedException($"Claim '{JwtRegisteredClaimNames.Sub}' is required");
        }

        /// <summary>
        /// Gets user Id as string from claims.
        /// It searches in next claims: 'sub', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'.
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ArgumentNullException(nameof(claimsPrincipal));

            return GetClaimValue(claimsPrincipal, JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Gets user email from claims.
        /// It searches in next claims: 'email', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'.
        /// </summary>
        public static string? GetUserEmail(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ArgumentNullException(nameof(claimsPrincipal));

            return GetClaimValue(claimsPrincipal, JwtRegisteredClaimNames.Email, ClaimTypes.Email);
        }
        
        private static string? GetClaimValue(ClaimsPrincipal? claimsPrincipal, params string[] claimNames)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            for (var i = 0; i < claimNames.Length; i++)
            {
                var currentValue = claimsPrincipal.FindFirstValue(claimNames[i]);
                if (!string.IsNullOrEmpty(currentValue))
                {
                    return currentValue;
                }
            }

            return null;
        }
    }
}
