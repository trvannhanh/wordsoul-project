namespace WordSoul.Application.Exceptions;

public sealed class ForbiddenException : WordSoulException
{
    public override string Code => ErrorCodes.AccessForbidden;

    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string resourceName, object ownerId)
        : base($"Access to '{resourceName}' owned by user '{ownerId}' is forbidden.") { }
}
