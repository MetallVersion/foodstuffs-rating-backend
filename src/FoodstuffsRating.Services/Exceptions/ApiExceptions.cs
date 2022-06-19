using System.Net;

namespace FoodstuffsRating.Services.Exceptions
{
    /// <summary>
    /// Represents base API exception, each exception type will be treated as specific HTTP status code in API response.
    /// </summary>
    public abstract class BaseApiException : Exception
    {
        public abstract HttpStatusCode ResponseStatusCode { get; }
        protected BaseApiException()
        {
        }

        protected BaseApiException(string message)
            : base(message)
        {
        }

        protected BaseApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents exception when request is not valid.
    /// Will be treated as 400 HTTP status code in API response.
    /// </summary>
    public class BadRequestException : BaseApiException
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
    public class ConflictException : BaseApiException
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
    public class ResourceNotFoundException : BaseApiException
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
}
