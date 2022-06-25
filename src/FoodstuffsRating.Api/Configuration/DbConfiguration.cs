using FoodstuffsRating.Api.EntityFramework;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Models.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoodstuffsRating.Api.Configuration
{
    public class DbConfiguration
    {
        public static void Configure(IServiceCollection services,
            IConfigurationRoot configuration,
            IWebHostEnvironment environment)
        {
            services.AddDbContext<BackendDbContext>(options =>
            {
                var dbOptions = configuration.GetSection("Backend:Database").Get<DatabaseOptions>();
                var connectionString = dbOptions.ConnectionString;

                options.UseSqlServer(connectionString, s =>
                {
                    s.CommandTimeout(dbOptions.TimeoutInSeconds);
                    if (dbOptions.RetryCount > 0)
                    {
                        s.EnableRetryOnFailure(dbOptions.RetryCount);
                    }
                });
                if (dbOptions.UseAzureAccessToken)
                {
                    options.AddInterceptors(new AzureDbConnectionInterceptor());
                }
            });

            if (environment.IsDevelopment())
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }
        }
    }
}
