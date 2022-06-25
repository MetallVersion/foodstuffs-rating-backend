using System;
using System.Net;

namespace FoodstuffsRating.Models.Exceptions
{
    /// <summary>
    /// Represents internal server error exception.
    /// Will be treated as 500 HTTP status code in API response.
    /// </summary>
    public class ApiException : Exception
    {
        public virtual HttpStatusCode ResponseStatusCode => HttpStatusCode.InternalServerError;

        protected ApiException()
        {
        }

        public ApiException(string message)
            : base(message)
        {
        }

        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents exception when request is not valid.
    /// Will be treated as 400 HTTP status code in API response.
    /// </summary>
    public class BadRequestException : ApiException
    {
        public const string DefaultMessage = "One or more validation errors occurred.";
        public override HttpStatusCode ResponseStatusCode => HttpStatusCode.BadRequest;

        public BadRequestException(string message)
            : base(message)
        {
        }

        protected BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents exception when state of resource or related resource in the conflict state.
    /// Will be treated as 409 HTTP status code in API response.
    /// </summary>
    public class ConflictException : ApiException
    {
        public override HttpStatusCode ResponseStatusCode => HttpStatusCode.Conflict;

        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents exception when main resource (in term of REST API) not found.
    /// Will be treated as 404 HTTP status code in API response.
    /// </summary>
    public class ResourceNotFoundException : ApiException
    {
        public override HttpStatusCode ResponseStatusCode => HttpStatusCode.NotFound;
        public string ResourceName { get; }
        public string ResourceIdentifier { get; }

        public ResourceNotFoundException(string resourceName, string resourceIdentifier)
        {
            this.ResourceName = resourceName;
            this.ResourceIdentifier = resourceIdentifier;
        }

        public override string Message =>
            $"Resource '{this.ResourceName}' not found by provided identifier: {this.ResourceIdentifier}";
    }

    /// <summary>
    /// Represents exception when client not authorized.
    /// Will be treated as 401 HTTP status code in API response.
    /// </summary>
    public class UnauthorizedException : ApiException
    {
        public override HttpStatusCode ResponseStatusCode => HttpStatusCode.Unauthorized;

        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
