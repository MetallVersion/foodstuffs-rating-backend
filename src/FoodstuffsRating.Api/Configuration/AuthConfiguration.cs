using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FoodstuffsRating.Common.Constants;
using FoodstuffsRating.Models.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FoodstuffsRating.Api.Configuration
{
    public static class AuthConfiguration
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            // remove mapping original JWT claim types to ASP.NET style, because we interested in original claims only
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // authentications
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
                .AddJwtBearer(AuthConstants.GoogleAuthScheme, jwtOptions =>
                {
                    var googleOptions = configuration.GetSection("Authentication:External:Google").Get<GoogleAuthOptions>();

                    jwtOptions.Authority = googleOptions.Authority;
                    jwtOptions.Audience = googleOptions.ClientId;
                    jwtOptions.MetadataAddress = googleOptions.OpenIdConfigurationUrl;
                    jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey = true;
                });
        }
    }
}
