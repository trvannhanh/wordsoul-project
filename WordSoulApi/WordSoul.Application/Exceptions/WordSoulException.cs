namespace WordSoul.Application.Exceptions;

/// <summary>
/// Base class for application-level failures that may safely cross the API boundary.
/// HTTP representation is intentionally owned by the API layer.
/// </summary>
public abstract class WordSoulException : Exception
{
    /// <summary>Stable code intended for client-side branching and telemetry.</summary>
    public abstract string Code { get; }

    protected WordSoulException(string message) : base(message) { }

    protected WordSoulException(string message, Exception innerException)
        : base(message, innerException) { }
}
