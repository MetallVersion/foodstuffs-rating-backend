using System;
using Microsoft.AspNetCore.Identity;

namespace FoodstuffsRating.Services.Auth
{
    /// <summary>
    /// ASP.NET Core Identity password hasher using the bcrypt password hashing algorithm.
    /// </summary>
    /// <typeparam name="TUser">your ASP.NET Core Identity user type (e.g. IdentityUser). User is not used by this implementation</typeparam>
    public sealed class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser>
        where TUser : class
    {
        /// <summary>
        /// The log2 of the number of rounds of hashing to apply. Defaults to 11 
        /// </summary>
        private const int WorkFactor = 11;

        /// <summary>
        /// Hashes a password using bcrypt.
        /// </summary>
        /// <param name="user">not used for this implementation</param>
        /// <param name="password">plaintext password</param>
        /// <returns>hashed password</returns>
        /// <exception cref="ArgumentNullException">missing plaintext password</exception>
        public string HashPassword(TUser user, string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verifies a plaintext password against a stored hash.
        /// </summary>
        /// <param name="user">not used for this implementation</param>
        /// <param name="hashedPassword">the stored, hashed password</param>
        /// <param name="providedPassword">the plaintext password to verify against the stored hash</param>
        /// <returns>If the password matches the stored password. Returns SuccessRehashNeeded if the work factor has changed</returns>
        /// <exception cref="ArgumentNullException">missing plaintext password or hashed password</exception>
        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword)) throw new ArgumentNullException(nameof(hashedPassword));
            if (string.IsNullOrWhiteSpace(providedPassword)) throw new ArgumentNullException(nameof(providedPassword));

            var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);

            if (isValid && BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor))
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            return isValid
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
