using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Services;
using FoodstuffsRating.Services.Auth;
using FoodstuffsRating.Services.AuthExternal;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodstuffsRating.Api.Configuration
{
    public static class DependencyInjectionConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            services.TryAddScoped(typeof(IBackendRepository<>), typeof(BackendRepository<>));

            ConfigureServices(services);
            ConfigureAuthServices(services);
            ConfigureAuthMisc(services);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<IUserService, UserService>();
        }

        private static void ConfigureAuthServices(IServiceCollection services)
        {
            services.TryAddScoped<IExternalProviderRegistrationService, ExternalProviderRegistrationService>();
            services.TryAddScoped<IUserRegistrationService, UserRegistrationService>();
            services.TryAddScoped<IUserTokenService, UserTokenService>();
            services.TryAddScoped<IAccessTokenService, AccessTokenService>();
            services.TryAddScoped<IRefreshTokenService, RefreshTokenService>();
        }

        private static void ConfigureAuthMisc(IServiceCollection services)
        {
            services.TryAddScoped<IJwtTokenProvider, JwtTokenProvider>();
            services.TryAddScoped<IRefreshTokenProvider, RefreshTokenProvider>();
            services.TryAddScoped(typeof(IPasswordHasher<>), typeof(BCryptPasswordHasher<>));
            services.TryAddScoped<IPasswordValidator, PasswordValidator>();
        }
    }
}
