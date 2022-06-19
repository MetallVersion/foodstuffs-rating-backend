using System.Net;
using FoodstuffsRating.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FoodstuffsRating.Api.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context,
            ProblemDetailsFactory problemDetailsFactory,
            IWebHostEnvironment env,
            ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            try
            {
                await this._next(context);
            }
            catch (BaseApiException apiException)
            {
                logger.LogWarning(apiException, apiException.Message);

                context.Response.StatusCode = (int)apiException.ResponseStatusCode;
                var problemResult = problemDetailsFactory.CreateProblemDetails(context,
                    statusCode: (int)apiException.ResponseStatusCode, detail: apiException.Message);

                await context.Response.WriteAsJsonAsync(problemResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var problemResult = problemDetailsFactory.CreateProblemDetails(context,
                    statusCode: context.Response.StatusCode);

                if (env.IsStaging() || env.IsProduction())
                {
                    await context.Response.WriteAsJsonAsync(problemResult);
                }
                else
                {
                    var exceptionDetails = new CustomExceptionDetails
                    {
                        Status = problemResult.Status,
                        Title = problemResult.Title,
                        Type = problemResult.Type,
                        Instance = problemResult.Instance,
                        Detail = ex.Message,
                        ExceptionDetails = ex.ToString()
                    };

                    await context.Response.WriteAsJsonAsync(exceptionDetails);
                }
            }
        }

        public class CustomExceptionDetails : ProblemDetails
        {
            public string? ExceptionDetails { get; set; }
        }
    }
}
