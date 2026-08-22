using WordSoul.Application.Exceptions;

namespace WordSoul.Api.Errors;

/// <summary>
/// Single source of truth for the public HTTP representation of WordSoul errors.
/// </summary>
public static class ApiErrorCatalog
{
    private const string ProblemTypeBaseUri = "https://wordsoul.app/errors";

    public static readonly ApiErrorDescriptor BadRequest = new(
        StatusCodes.Status400BadRequest,
        ErrorCodes.BadRequest,
        $"{ProblemTypeBaseUri}/bad-request",
        "Bad Request",
        "The request could not be processed.");

    public static readonly ApiErrorDescriptor AuthenticationRequired = new(
        StatusCodes.Status401Unauthorized,
        ErrorCodes.AuthenticationRequired,
        $"{ProblemTypeBaseUri}/unauthorized",
        "Unauthorized",
        "Authentication is required to access this resource.");

    public static readonly ApiErrorDescriptor AccessForbidden = new(
        StatusCodes.Status403Forbidden,
        ErrorCodes.AccessForbidden,
        $"{ProblemTypeBaseUri}/forbidden",
        "Forbidden",
        "You do not have permission to access this resource.");

    public static readonly ApiErrorDescriptor ResourceNotFound = new(
        StatusCodes.Status404NotFound,
        ErrorCodes.ResourceNotFound,
        $"{ProblemTypeBaseUri}/not-found",
        "Resource Not Found",
        "The requested resource was not found.");

    public static readonly ApiErrorDescriptor MethodNotAllowed = new(
        StatusCodes.Status405MethodNotAllowed,
        ApiErrorCodes.MethodNotAllowed,
        $"{ProblemTypeBaseUri}/method-not-allowed",
        "Method Not Allowed",
        "The HTTP method is not supported for this resource.");

    public static readonly ApiErrorDescriptor Conflict = new(
        StatusCodes.Status409Conflict,
        ErrorCodes.Conflict,
        $"{ProblemTypeBaseUri}/conflict",
        "Conflict",
        "The request conflicts with the current resource state.");

    public static readonly ApiErrorDescriptor UnsupportedMediaType = new(
        StatusCodes.Status415UnsupportedMediaType,
        ApiErrorCodes.UnsupportedMediaType,
        $"{ProblemTypeBaseUri}/unsupported-media-type",
        "Unsupported Media Type",
        "The request content type is not supported.");

    public static readonly ApiErrorDescriptor ValidationFailed = new(
        StatusCodes.Status422UnprocessableEntity,
        ErrorCodes.ValidationFailed,
        $"{ProblemTypeBaseUri}/validation",
        "Validation Failed",
        "One or more validation errors occurred.");

    public static readonly ApiErrorDescriptor RateLimitExceeded = new(
        StatusCodes.Status429TooManyRequests,
        ErrorCodes.RateLimitExceeded,
        $"{ProblemTypeBaseUri}/rate-limit",
        "Too Many Requests",
        "You have exceeded the request limit. Please try again later.");

    public static readonly ApiErrorDescriptor ExternalServiceFailed = new(
        StatusCodes.Status502BadGateway,
        ErrorCodes.ExternalServiceFailed,
        $"{ProblemTypeBaseUri}/external-service",
        "External Service Error",
        "An external service could not complete the request.");

    public static readonly ApiErrorDescriptor InvalidGameAction = new(
        StatusCodes.Status400BadRequest,
        ErrorCodes.InvalidGameAction,
        $"{ProblemTypeBaseUri}/game-logic",
        "Invalid Game Action",
        "The requested game action is not valid.");

    public static readonly ApiErrorDescriptor InternalServerError = new(
        StatusCodes.Status500InternalServerError,
        ApiErrorCodes.InternalServerError,
        $"{ProblemTypeBaseUri}/internal-server-error",
        "Internal Server Error",
        "An unexpected error occurred. Please try again later.");

    public static readonly ApiErrorDescriptor RequestFailed = new(
        StatusCodes.Status400BadRequest,
        ApiErrorCodes.RequestFailed,
        $"{ProblemTypeBaseUri}/request-failed",
        "Request Failed",
        "The request could not be completed.");

    private static readonly IReadOnlyDictionary<string, ApiErrorDescriptor> ByCode =
        new Dictionary<string, ApiErrorDescriptor>(StringComparer.Ordinal)
        {
            [BadRequest.Code] = BadRequest,
            [AuthenticationRequired.Code] = AuthenticationRequired,
            [AccessForbidden.Code] = AccessForbidden,
            [ResourceNotFound.Code] = ResourceNotFound,
            [Conflict.Code] = Conflict,
            [ValidationFailed.Code] = ValidationFailed,
            [RateLimitExceeded.Code] = RateLimitExceeded,
            [ExternalServiceFailed.Code] = ExternalServiceFailed,
            [InvalidGameAction.Code] = InvalidGameAction
        };

    public static ApiErrorDescriptor ForApplicationCode(string code) =>
        ByCode.GetValueOrDefault(code, InternalServerError);

    public static ApiErrorDescriptor ForStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => BadRequest,
        StatusCodes.Status401Unauthorized => AuthenticationRequired,
        StatusCodes.Status403Forbidden => AccessForbidden,
        StatusCodes.Status404NotFound => ResourceNotFound,
        StatusCodes.Status405MethodNotAllowed => MethodNotAllowed,
        StatusCodes.Status409Conflict => Conflict,
        StatusCodes.Status415UnsupportedMediaType => UnsupportedMediaType,
        StatusCodes.Status422UnprocessableEntity => ValidationFailed,
        StatusCodes.Status429TooManyRequests => RateLimitExceeded,
        StatusCodes.Status502BadGateway => ExternalServiceFailed,
        _ when status >= StatusCodes.Status500InternalServerError =>
            InternalServerError with { Status = status },
        _ => RequestFailed with { Status = status }
    };
}
