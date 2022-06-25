using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Models.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Services.Auth
{
    public interface IUserRegistrationService
    {
        Task<User> RegisterAsync(UserRegistrationRequest registrationRequest);
        Task<User?> GetUserByPassword(string email, string password);
    }

    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IBackendRepository<User> _userRepository;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<UserTokenService> _logger;

        public UserRegistrationService(IBackendRepository<User> userRepository,
            IPasswordValidator passwordValidator,
            IPasswordHasher<User> passwordHasher,
            IMapper mapper,
            ILogger<UserTokenService> logger)
        {
            this._userRepository = userRepository;
            this._passwordValidator = passwordValidator;
            this._passwordHasher = passwordHasher;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<User> RegisterAsync(UserRegistrationRequest registrationRequest)
        {
            if (registrationRequest == null) throw new ArgumentNullException(nameof(registrationRequest));

            var existingUser = await this._userRepository.GetAsync(x => x.Email == registrationRequest.Email);
            if (existingUser != null)
            {
                this._logger.LogWarning("Attempt to register user with duplicate email, email: {userEmail}",
                    registrationRequest.Email);
                throw new InvalidOperationException("User already registered");
            }

            var validationResult = this._passwordValidator.Validate(registrationRequest.Password);
            if (!validationResult.Succeeded)
            {
                this._logger.LogInformation($"Password validation fails, email: {registrationRequest.Email}, " +
                    $"errors: {string.Join(", ", validationResult.Errors.Select(x => x.Description))}");

                // TODO: provide full list of errors
                throw new BadRequestException(validationResult.Errors.First().Description);
            }

            var user = this._mapper.Map<User>(registrationRequest);
            user.Id = Guid.NewGuid();
            this._logger.LogTrace($"New UserId generated: {user.Id}");

            user.PasswordHash = this._passwordHasher.HashPassword(user, registrationRequest.Password);

            await this._userRepository.AddAsync(user);

            this._logger.LogInformation("New user was registered with Id: {userId}, email: {userEmail}",
                user.Id, registrationRequest.Email);

            return user;
        }

        public async Task<User?> GetUserByPassword(string email, string password)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var user = await this._userRepository.GetAsync(x => x.Email == email, asNoTracking: false);
            if (user == null)
            {
                this._logger.LogWarning("User was not found by provided email: {userEmail}", email);

                return null;
            }

            var passwordVerificationResult =  this._passwordHasher.VerifyHashedPassword(user,
                user.PasswordHash, password);

            if (passwordVerificationResult == PasswordVerificationResult.Success)
            {
                return user;
            }
            if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                await this.RehashPasswordAsync(user, password);

                return user;
            }

            return null;
        }

        private async Task RehashPasswordAsync(User user, string password)
        {
            user.PasswordHash = this._passwordHasher.HashPassword(user, password);

            await this._userRepository.UpdateAsync(user);
        }
    }
}