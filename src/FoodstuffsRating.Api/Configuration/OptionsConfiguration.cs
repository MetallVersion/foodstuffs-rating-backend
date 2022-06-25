using FoodstuffsRating.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstuffsRating.Api.Configuration
{
    public static class OptionsConfiguration
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<DatabaseOptions>()
                .Configure(o => configuration.GetSection("Backend:Database").Bind(o))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AuthOptions>()
                .Configure(o => configuration.GetSection("Authentication:Own").Bind(o))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<PasswordRequirementsOptions>()
                .Configure(o => configuration.GetSection("Registration:PasswordOptions").Bind(o))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<GoogleAuthOptions>()
                .Configure(o => configuration.GetSection("Authentication:External:Google").Bind(o))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}
