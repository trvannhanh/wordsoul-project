namespace WordSoul.Application.Exceptions;

public sealed class ConflictException(string message) : WordSoulException(message)
{
    public override string Code => ErrorCodes.Conflict;
}
