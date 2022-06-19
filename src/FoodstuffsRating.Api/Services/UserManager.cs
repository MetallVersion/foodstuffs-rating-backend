using AutoMapper;
using FoodstuffsRating.Api.Dto;
using FoodstuffsRating.Api.OAuth;
using FoodstuffsRating.Api.Options;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Services.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FoodstuffsRating.Api.Services
{
    public interface IUserManager
    {
        Task<User> RegisterAsync(RegisterUserRequest request);
        Task<User?> GetUserByPassword(string email, string password);
        Task<TokenResponse> IssueNewTokenAsync(Guid userId);
    }

    public class UserManager : IUserManager
    {
        private readonly IBackendRepository<User> _userRepository;
        private readonly IBackendRepository<UserRefreshToken> _refreshTokenRepository;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly AuthOptions _authOptions;
        private readonly IMapper _mapper;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IBackendRepository<User> userRepository,
            IBackendRepository<UserRefreshToken> refreshTokenRepository,
            IPasswordValidator passwordValidator,
            IPasswordHasher<User> passwordHasher,
            IJwtTokenProvider jwtTokenProvider,
            IRefreshTokenProvider refreshTokenProvider,
            IOptions<AuthOptions> authOptions,
            IMapper mapper,
            ILogger<UserManager> logger)
        {
            this._userRepository = userRepository;
            this._refreshTokenRepository = refreshTokenRepository;
            this._passwordValidator = passwordValidator;
            this._passwordHasher = passwordHasher;
            this._jwtTokenProvider = jwtTokenProvider;
            this._refreshTokenProvider = refreshTokenProvider;
            this._mapper = mapper;
            this._authOptions = authOptions.Value;
            this._logger = logger;
        }

        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var existingUser = await this._userRepository.GetAsync(x => x.Email == request.Email);
            if (existingUser != null)
            {
                this._logger.LogWarning("Attempt to register user with duplicate email, email: {userEmail}",
                    request.Email);
                throw new InvalidOperationException("User already registered");
            }

            var validationResult = this._passwordValidator.Validate(request.Password);
            if (!validationResult.Succeeded)
            {
                this._logger.LogInformation($"Password validation fails, email: {request.Email}, " +
                    $"errors: {string.Join(", ", validationResult.Errors.Select(x => x.Description))}");

                // TODO: provide full list of errors
                throw new BadRequestException(validationResult.Errors.First().Description);
            }

            var user = this._mapper.Map<User>(request);
            user.Id = Guid.NewGuid();
            user.PasswordHash = this._passwordHasher.HashPassword(user, request.Password);

            await this._userRepository.AddAsync(user);

            this._logger.LogInformation("New user was registered with Id: {userId}, email: {userEmail}",
                user.Id, request.Email);

            return user;
        }

        public async Task<TokenResponse> IssueNewTokenAsync(Guid userId)
        {
            var user = await this._userRepository.GetAsync(x => x.Id == userId, asNoTracking: false);
            if (user == null)
            {
                this._logger.LogWarning("User was not found by provided Id: {userId}", userId);

                throw new InvalidOperationException("User was not found");
            }

            var utcNow = DateTime.UtcNow;

            int expiresIn = (int)TimeSpan.FromMinutes(
                this._authOptions.Jwt.ExpirationInMinutes).TotalSeconds;
            string jwtToken = this._jwtTokenProvider.CreateToken(user.Id, user.Email);

            int refreshTokenExpiresIn = (int)TimeSpan.FromMinutes(
                this._authOptions.RefreshToken.ExpirationInMinutes).TotalSeconds;
            string refreshToken = this._refreshTokenProvider.GenerateRefreshToken();

            var newRefreshToken = new UserRefreshToken
            {
                RefreshToken = refreshToken,
                IsActive = true,
                UserId = userId,
                ExpiresAtUtc = utcNow.AddMinutes(this._authOptions.RefreshToken.ExpirationInMinutes)
            };
            await this._refreshTokenRepository.AddAsync(newRefreshToken);

            user.LastLoginDateUtc = utcNow;
            await this._userRepository.UpdateAsync(user);

            var tokenResponse = new TokenResponse
            {
                AccessToken = jwtToken,
                ExpiresIn = expiresIn,
                RefreshToken = refreshToken,
                RefreshTokenExpiresIn = refreshTokenExpiresIn,
            };

            return tokenResponse;
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
