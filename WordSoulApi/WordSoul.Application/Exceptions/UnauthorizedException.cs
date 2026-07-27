namespace WordSoul.Application.Exceptions;

public sealed class UnauthorizedException(
    string message = "Authentication is required.") : WordSoulException(message)
{
    public override string Code => ErrorCodes.AuthenticationRequired;
}
