using System.Net;

namespace WordSoul.Application.Exceptions;

// ─────────────────────────────────────────────────────────────
//  Base Exception
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Base class for all domain/application-level exceptions in WordSoul.
/// Every concrete subtype must declare its own HTTP status code and a
/// short RFC 7807-compatible <see cref="Title"/>.
/// </summary>
public abstract class WordSoulException : Exception
{
    /// <summary>HTTP status code that will be returned to the caller.</summary>
    public abstract HttpStatusCode StatusCode { get; }

    /// <summary>Short, human-readable title for the problem details response.</summary>
    public abstract string Title { get; }

    /// <summary>
    /// A URI reference that identifies the problem type.
    /// Follows the pattern <c>https://wordsoul.app/errors/{slug}</c>.
    /// </summary>
    public abstract string Type { get; }

    protected WordSoulException(string message) : base(message) { }

    protected WordSoulException(string message, Exception innerException)
        : base(message, innerException) { }
}

// ─────────────────────────────────────────────────────────────
//  404 – Not Found
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when a requested entity cannot be located by its identifier.
/// </summary>
/// <example>throw new NotFoundException(nameof(Pet), petId);</example>
public sealed class NotFoundException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public override string Title => "Resource Not Found";
    public override string Type => "https://wordsoul.app/errors/not-found";

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.") { }

    public NotFoundException(string message)
        : base(message) { }
}

// ─────────────────────────────────────────────────────────────
//  422 – Validation / Business Rule Failure
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when input data or a business rule fails validation.
/// Supports a field-level <see cref="Errors"/> dictionary compatible
/// with <c>ValidationProblemDetails</c>.
/// </summary>
public sealed class ValidationException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.UnprocessableEntity;
    public override string Title => "Validation Failed";
    public override string Type => "https://wordsoul.app/errors/validation";

    /// <summary>
    /// Field-level error messages.
    /// Key = field name (camelCase), Value = array of error messages for that field.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    public ValidationException(string field, string error)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [field] = new[] { error }
        };
    }
}

// ─────────────────────────────────────────────────────────────
//  409 – Conflict
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when an operation would create a duplicate resource or result
/// in a conflicting system state (e.g., user already in matchmaking queue).
/// </summary>
public sealed class ConflictException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public override string Title => "Conflict";
    public override string Type => "https://wordsoul.app/errors/conflict";

    public ConflictException(string message) : base(message) { }
}

// ─────────────────────────────────────────────────────────────
//  403 – Forbidden
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when an authenticated user attempts to access a resource they
/// are not authorized to access (e.g., viewing another user's pet).
/// </summary>
public sealed class ForbiddenException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    public override string Title => "Forbidden";
    public override string Type => "https://wordsoul.app/errors/forbidden";

    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string resourceName, object ownerId)
        : base($"Access to '{resourceName}' owned by user '{ownerId}' is forbidden.") { }
}

// ─────────────────────────────────────────────────────────────
//  401 – Unauthorized
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when a request is missing or contains an invalid/expired token.
/// </summary>
public sealed class UnauthorizedException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public override string Title => "Unauthorized";
    public override string Type => "https://wordsoul.app/errors/unauthorized";

    public UnauthorizedException(string message = "Authentication is required.") : base(message) { }
}

// ─────────────────────────────────────────────────────────────
//  502 – External Service Failure
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when an external third-party service (Cloudinary, Firebase,
/// Azure Speech, Gemini AI, Unsplash, SendGrid) returns an unexpected
/// error or times out.
/// </summary>
public sealed class ExternalServiceException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadGateway;
    public override string Title => "External Service Error";
    public override string Type => "https://wordsoul.app/errors/external-service";

    /// <summary>Name of the external service that failed (e.g., "Cloudinary").</summary>
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base($"[{serviceName}] {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"[{serviceName}] {message}", innerException)
    {
        ServiceName = serviceName;
    }
}

// ─────────────────────────────────────────────────────────────
//  400 – Game Logic Violation
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when a domain-specific game rule is violated (e.g., pet HP is
/// already 0, invalid battle action sequence).
/// </summary>
public sealed class GameLogicException : WordSoulException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public override string Title => "Invalid Game Action";
    public override string Type => "https://wordsoul.app/errors/game-logic";

    public GameLogicException(string message) : base(message) { }
}

// ─────────────────────────────────────────────────────────────
//  429 – Rate Limit
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Thrown when the caller has exceeded the allowed request rate for an
/// AI or external service endpoint.
/// </summary>
public sealed class RateLimitException : WordSoulException
{
    public override HttpStatusCode StatusCode => (HttpStatusCode)429;
    public override string Title => "Too Many Requests";
    public override string Type => "https://wordsoul.app/errors/rate-limit";

    /// <summary>
    /// Optional: how many seconds the caller should wait before retrying.
    /// Populate this to emit a <c>Retry-After</c> header.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    public RateLimitException(string message, int? retryAfterSeconds = null)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
