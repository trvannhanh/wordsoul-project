namespace WordSoul.Application.Exceptions;

/// <summary>
/// Stable, machine-readable error codes exposed by the public API.
/// Clients should branch on these values instead of parsing error messages.
/// </summary>
public static class ErrorCodes
{
    public const string BadRequest = "BAD_REQUEST";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string AccessForbidden = "ACCESS_FORBIDDEN";
    public const string AuthenticationRequired = "AUTHENTICATION_REQUIRED";
    public const string ExternalServiceFailed = "EXTERNAL_SERVICE_FAILED";
    public const string InvalidGameAction = "INVALID_GAME_ACTION";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
}
