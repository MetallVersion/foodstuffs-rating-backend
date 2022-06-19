using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoodstuffsRating.Api.Interceptors;
using FoodstuffsRating.Api.Middlewares;
using FoodstuffsRating.Api.OAuth;
using FoodstuffsRating.Api.Options;
using FoodstuffsRating.Api.Services;
using FoodstuffsRating.Api.Startup;
using FoodstuffsRating.Data.Dal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Azure KeyVault
builder.Host.ConfigureAppConfiguration((_, config) =>
{
    var settings = config.Build();

    string keyVaultEndpoint = settings["AzureKeyVaultEndpoint"];

    var secretClient = new SecretClient(new Uri(keyVaultEndpoint),
        new DefaultAzureCredential());

    config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
});

var services = builder.Services;
var configuration = builder.Configuration;


// AppInsight
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.

services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

AutoMapperConfiguration.Configure(services);


// options
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

// auth
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
    {
        var jwtConfig = configuration.GetSection("Authentication:Own:Jwt").Get<AuthOptions.JwtOptions>();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.IssuerSigningKey));
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
    //{
    //    var googleOptions = configuration.GetSection("Authentication:External:Google").Get<GoogleAuthOptions>();

    //    jwtOptions.Authority = googleOptions.Authority;
    //    jwtOptions.Audience = googleOptions.ClientId;
    //    jwtOptions.MetadataAddress = googleOptions.OpenIdConfigurationUrl;
    //    jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey = true;
    //})
    ;


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

if (builder.Environment.IsDevelopment())
{
    services.AddDatabaseDeveloperPageExceptionFilter();
}

services.AddSingleton(new JsonOptions
{
    SerializerOptions =
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    }
});

// services
services.TryAddScoped<IJwtTokenProvider, JwtTokenProvider>();
services.TryAddScoped<IRefreshTokenProvider, RefreshTokenProvider>();
services.AddTransient(typeof(IPasswordHasher<>), typeof(BCryptPasswordHasher<>));
services.AddTransient<IPasswordValidator, PasswordValidator>();

services.AddTransient(typeof(IBackendRepository<>), typeof(BackendRepository<>));
services.AddScoped<IUserManager, UserManager>();

// healthchecks
services.AddHealthChecks()
    .AddDbContextCheck<BackendDbContext>(name: "Sql-BackendDb");

//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

app.MapControllers();

app.UseHealthChecks("/healthcheck");

// apply EF Core db migrations
using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackendDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
