namespace WordSoul.Application.Exceptions;

public sealed class BadRequestException(string message) : WordSoulException(message)
{
    public override string Code => ErrorCodes.BadRequest;
}
