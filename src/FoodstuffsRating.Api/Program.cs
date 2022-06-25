using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoodstuffsRating.Api.Configuration;
using FoodstuffsRating.Api.Middlewares;
using FoodstuffsRating.Data.Dal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

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

MapperConfiguration.Configure(services);
OptionsConfiguration.Configure(services, configuration);
AuthConfiguration.Configure(services, configuration);
DbConfiguration.Configure(services, configuration, builder.Environment);
DependencyInjectionConfiguration.Configure(services);
HealthChecksConfiguration.Configure(services);

// global JsonOptions
services.AddSingleton(new JsonOptions
{
    SerializerOptions =
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
        RequestPath = "/ExternalLogins",
        EnableDefaultFiles = true
    });
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
