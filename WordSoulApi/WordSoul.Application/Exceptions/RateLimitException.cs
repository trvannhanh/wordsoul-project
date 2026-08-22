namespace WordSoul.Application.Exceptions;

public sealed class RateLimitException : WordSoulException
{
    public override string Code => ErrorCodes.RateLimitExceeded;

    public int? RetryAfterSeconds { get; }

    public RateLimitException(string message, int? retryAfterSeconds = null)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
