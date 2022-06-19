using FoodstuffsRating.Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Api.Services
{
    public interface IPasswordValidator
    {
        /// <summary>
        /// Validates a password as an asynchronous operation.
        /// </summary>
        /// <param name="password">The password supplied for validation</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        IdentityResult Validate(string password);
    }

    /// <summary>
    /// Copy of Asp.Net default PasswordValidator implementation but without dependency to UserManager and TUser
    /// </summary>
    public class PasswordValidator : IPasswordValidator
    {
        private readonly PasswordRequirementsOptions _options;

        /// <summary>
        /// Constructions a new instance of <see cref="PasswordValidator"/>.
        /// </summary>
        /// <param name="passwordOptions"></param>
        /// <param name="errors">The <see cref="IdentityErrorDescriber"/> to retrieve error text from.</param>
        public PasswordValidator(IOptions<PasswordRequirementsOptions> passwordOptions,
            IdentityErrorDescriber? errors = null)
        {
            this._options = passwordOptions.Value;
            this.Describer = errors ?? new IdentityErrorDescriber();
        }

        /// <summary>
        /// Gets the <see cref="IdentityErrorDescriber"/> used to supply error text.
        /// </summary>
        /// <value>The <see cref="IdentityErrorDescriber"/> used to supply error text.</value>
        protected IdentityErrorDescriber Describer { get; set; }

        /// <summary>
        /// Validates a password as an asynchronous operation.
        /// </summary>
        /// <param name="password">The password supplied for validation</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public IdentityResult Validate(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var errors = new List<IdentityError>();
            if (string.IsNullOrWhiteSpace(password) || password.Length < this._options.RequiredLength)
            {
                errors.Add(this.Describer.PasswordTooShort(this._options.RequiredLength));
            }
            if (password.Length > this._options.MaxLength)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooLong",
                    Description = $"Passwords can be maximum {this._options.MaxLength} characters."
                });
            }
            if (this._options.RequireNonAlphanumeric && password.All(this.IsLetterOrDigit))
            {
                errors.Add(this.Describer.PasswordRequiresNonAlphanumeric());
            }
            if (this._options.RequireDigit && !password.Any(this.IsDigit))
            {
                errors.Add(this.Describer.PasswordRequiresDigit());
            }
            if (this._options.RequireLowercase && !password.Any(this.IsLower))
            {
                errors.Add(this.Describer.PasswordRequiresLower());
            }
            if (this._options.RequireUppercase && !password.Any(this.IsUpper))
            {
                errors.Add(this.Describer.PasswordRequiresUpper());
            }
            if (this._options.RequiredUniqueChars >= 1 && password.Distinct().Count() < this._options.RequiredUniqueChars)
            {
                errors.Add(this.Describer.PasswordRequiresUniqueChars(this._options.RequiredUniqueChars));
            }

            return errors.Count == 0
                ? IdentityResult.Success
                : IdentityResult.Failed(errors.ToArray());
        }

        /// <summary>
        /// Returns a flag indicating whether the supplied character is a digit.
        /// </summary>
        /// <param name="c">The character to check if it is a digit.</param>
        /// <returns>True if the character is a digit, otherwise false.</returns>
        protected virtual bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Returns a flag indicating whether the supplied character is a lower case ASCII letter.
        /// </summary>
        /// <param name="c">The character to check if it is a lower case ASCII letter.</param>
        /// <returns>True if the character is a lower case ASCII letter, otherwise false.</returns>
        protected virtual bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        /// <summary>
        /// Returns a flag indicating whether the supplied character is an upper case ASCII letter.
        /// </summary>
        /// <param name="c">The character to check if it is an upper case ASCII letter.</param>
        /// <returns>True if the character is an upper case ASCII letter, otherwise false.</returns>
        protected virtual bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        /// <summary>
        /// Returns a flag indicating whether the supplied character is an ASCII letter or digit.
        /// </summary>
        /// <param name="c">The character to check if it is an ASCII letter or digit.</param>
        /// <returns>True if the character is an ASCII letter or digit, otherwise false.</returns>
        protected virtual bool IsLetterOrDigit(char c)
        {
            return this.IsUpper(c) || this.IsLower(c) || this.IsDigit(c);
        }
    }
}
