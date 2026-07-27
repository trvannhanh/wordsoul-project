namespace WordSoul.Api.Errors;

/// <summary>Stable codes for failures produced by the HTTP/API layer.</summary>
public static class ApiErrorCodes
{
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string RequestFailed = "REQUEST_FAILED";
}
