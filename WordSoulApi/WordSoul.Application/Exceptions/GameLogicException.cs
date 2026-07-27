namespace WordSoul.Application.Exceptions;

public sealed class GameLogicException(string message) : WordSoulException(message)
{
    public override string Code => ErrorCodes.InvalidGameAction;
}
