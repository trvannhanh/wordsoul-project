namespace WordSoul.Application.Exceptions;

public sealed class ExternalServiceException : WordSoulException
{
    public override string Code => ErrorCodes.ExternalServiceFailed;

    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base($"[{serviceName}] {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(
        string serviceName,
        string message,
        Exception innerException)
        : base($"[{serviceName}] {message}", innerException)
    {
        ServiceName = serviceName;
    }
}
