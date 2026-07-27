namespace WordSoul.Application.Exceptions;

public sealed class NotFoundException : WordSoulException
{
    public override string Code => ErrorCodes.ResourceNotFound;

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}
