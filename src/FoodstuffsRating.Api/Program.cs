using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.

services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

AutoMapperConfiguration.Configure(services);


// options
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
    options.UseSqlServer(configuration.GetValue<string>("Backend:Database:ConnectionString"));
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

// app.UseHealthChecks("/healthcheck");// TODO:

// apply EF Core db migrations
using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackendDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
