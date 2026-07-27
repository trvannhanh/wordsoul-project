namespace WordSoul.Api.Errors;

public sealed record ApiErrorDescriptor(
    int Status,
    string Code,
    string Type,
    string Title,
    string Detail);
