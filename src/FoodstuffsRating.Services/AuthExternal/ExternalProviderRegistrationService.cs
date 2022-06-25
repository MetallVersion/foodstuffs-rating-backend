using System;
using System.Threading.Tasks;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Models.Auth;
using FoodstuffsRating.Models.Exceptions;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Services.AuthExternal
{
    public interface IExternalProviderRegistrationService
    {
        Task<User> RegisterAsync(ExternalProviderRegistrationModel model);

        Task<User?> GetUserByExternalLoginAsync(string externalUserId,
            string userEmail, ExternalLoginProvider provider);
    }

    public class ExternalProviderRegistrationService : IExternalProviderRegistrationService
    {
        private readonly IBackendRepository<User> _userRepository;
        private readonly IBackendRepository<UserExternalLogin> _externalLoginRepository;
        private readonly ILogger<ExternalProviderRegistrationService> _logger;

        public ExternalProviderRegistrationService(IBackendRepository<User> userRepository,
            IBackendRepository<UserExternalLogin> externalLoginRepository,
            ILogger<ExternalProviderRegistrationService> logger)
        {
            this._userRepository = userRepository;
            this._externalLoginRepository = externalLoginRepository;
            this._logger = logger;
        }

        public async Task<User> RegisterAsync(ExternalProviderRegistrationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.Email)) throw new ArgumentException("Email is required", nameof(model));
            if (string.IsNullOrWhiteSpace(model.ExternalUserId)) throw new ArgumentException("ExternalUserId is required", nameof(model));

            await this.EnsureUserNotExistsByExternalIdAsync(model);

            await this.EnsureUserNotExistsByEmailAsync(model);

            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Id = Guid.NewGuid()
            };
            this._logger.LogTrace($"New UserId generated: {user.Id}");

            await this._userRepository.AddAsync(user);
            using var ls2 = this._logger.BeginScope("{userId}", user.Id);
            this._logger.LogTrace("New User created from external login provider");

            var externalLogin = new UserExternalLogin
            {
                LoginProvider = model.LoginProvider,
                ExternalUserId = model.ExternalUserId,
                UserId = user.Id
            };
            await this._externalLoginRepository.AddAsync(externalLogin);
            this._logger.LogTrace("UserExternalLogin created");

            return user;
        }

        public async Task<User?> GetUserByExternalLoginAsync(string externalUserId,
            string userEmail, ExternalLoginProvider provider)
        {
            var externalLogin = await this._externalLoginRepository.GetAsync(
                x => x.ExternalUserId == externalUserId && x.LoginProvider == provider,
                asNoTracking: true, x => x.User);
            if (externalLogin == null)
            {
                this._logger.LogWarning($"User not found by external userId: {externalUserId}");

                return null;
            }

            var user = externalLogin.User;
            if (!string.Equals(user.Email, userEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                this._logger.LogWarning($"User email mismatch, " +
                    $"external login provider email: {userEmail}, " +
                    $"registered user email: {user.Email}");

                return null;
            }

            // TODO: verify that user confirmed if necessary

            return user;
        }

        private async Task EnsureUserNotExistsByEmailAsync(ExternalProviderRegistrationModel model)
        {
            bool isUserExistsByEmail = await this._userRepository.AnyAsync(x => x.Email == model.Email);
            if (isUserExistsByEmail)
            {
                this._logger.LogWarning("User with provided email already registered");

                throw new ConflictException("The email has already been taken");
            }
        }

        private async Task EnsureUserNotExistsByExternalIdAsync(ExternalProviderRegistrationModel model)
        {
            bool isUserExistsByExternalId = await this._externalLoginRepository.AnyAsync(
                x => x.ExternalUserId == model.ExternalUserId && x.LoginProvider == model.LoginProvider);
            if (isUserExistsByExternalId)
            {
                this._logger.LogWarning("User already registered by external userId");

                throw new ConflictException("The user has already been registered");
            }
        }
    }
}
