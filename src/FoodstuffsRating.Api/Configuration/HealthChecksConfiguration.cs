using FoodstuffsRating.Data.Dal;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstuffsRating.Api.Configuration
{
    public static class HealthChecksConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<BackendDbContext>(name: "Sql-BackendDb");
        }
    }
}
