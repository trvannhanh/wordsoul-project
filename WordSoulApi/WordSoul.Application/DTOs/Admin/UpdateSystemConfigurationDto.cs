namespace WordSoul.Application.DTOs.Admin;

public sealed record UpdateSystemConfigurationDto(
    string Key,
    string Value);
