using System.Globalization;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services.SRS;

public sealed record SrsAlgorithmSettings(
    int PolicyVersion,
    double MinEaseFactor,
    double MaxEaseFactor,
    double DefaultEaseFactor,
    int FirstIntervalDays,
    int SecondIntervalDays,
    int MasteredIntervalDays,
    decimal RetentionBonusPerRepetition,
    decimal RetentionBonusMaximum)
{
    public const string PolicyVersionKey = "SrsPolicyVersion";
    public const string MinEaseFactorKey = "SrsMinEf";
    public const string MaxEaseFactorKey = "SrsMaxEf";
    public const string DefaultEaseFactorKey = "SrsDefaultEf";
    public const string FirstIntervalKey = "SrsInitialInterval1";
    public const string SecondIntervalKey = "SrsInitialInterval2";
    public const string MasteredIntervalKey = "SrsMasteredIntervalDays";
    public const string RetentionBonusPerRepetitionKey =
        "SrsRetentionBonusPerRepetition";
    public const string RetentionBonusMaximumKey = "SrsRetentionBonusMax";

    public static readonly IReadOnlySet<string> AlgorithmKeys =
        new HashSet<string>(
            [
                MinEaseFactorKey,
                MaxEaseFactorKey,
                DefaultEaseFactorKey,
                FirstIntervalKey,
                SecondIntervalKey,
                MasteredIntervalKey,
                RetentionBonusPerRepetitionKey,
                RetentionBonusMaximumKey
            ],
            StringComparer.OrdinalIgnoreCase);

    public static SrsAlgorithmSettings Default { get; } = new(
        PolicyVersion: 1,
        MinEaseFactor: 1.3,
        MaxEaseFactor: 4.0,
        DefaultEaseFactor: 2.5,
        FirstIntervalDays: 1,
        SecondIntervalDays: 6,
        MasteredIntervalDays: 21,
        RetentionBonusPerRepetition: 2m,
        RetentionBonusMaximum: 20m);

    public static SrsAlgorithmSettings FromConfigurations(
        IEnumerable<SystemConfiguration> configurations)
    {
        var values = configurations.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.Ordinal);
        var defaults = Default;

        var settings = new SrsAlgorithmSettings(
            Read(values, PolicyVersionKey, defaults.PolicyVersion),
            Read(values, MinEaseFactorKey, defaults.MinEaseFactor),
            Read(values, MaxEaseFactorKey, defaults.MaxEaseFactor),
            Read(values, DefaultEaseFactorKey, defaults.DefaultEaseFactor),
            Read(values, FirstIntervalKey, defaults.FirstIntervalDays),
            Read(values, SecondIntervalKey, defaults.SecondIntervalDays),
            Read(values, MasteredIntervalKey, defaults.MasteredIntervalDays),
            Read(
                values,
                RetentionBonusPerRepetitionKey,
                defaults.RetentionBonusPerRepetition),
            Read(
                values,
                RetentionBonusMaximumKey,
                defaults.RetentionBonusMaximum));

        settings.Validate();
        return settings;
    }

    public void Validate()
    {
        if (PolicyVersion <= 0)
            throw new InvalidOperationException("SRS policy version must be positive.");
        if (MinEaseFactor <= 0 || MinEaseFactor > MaxEaseFactor)
        {
            throw new InvalidOperationException(
                "SRS minimum ease factor must be positive and not exceed the maximum.");
        }
        if (DefaultEaseFactor < MinEaseFactor
            || DefaultEaseFactor > MaxEaseFactor)
        {
            throw new InvalidOperationException(
                "SRS default ease factor must be between the minimum and maximum.");
        }
        if (FirstIntervalDays < 0
            || SecondIntervalDays < FirstIntervalDays)
        {
            throw new InvalidOperationException(
                "SRS intervals must be non-negative and ordered.");
        }
        if (MasteredIntervalDays < SecondIntervalDays)
        {
            throw new InvalidOperationException(
                "SRS mastered interval cannot be shorter than the second interval.");
        }
        if (RetentionBonusPerRepetition < 0
            || RetentionBonusMaximum < 0)
        {
            throw new InvalidOperationException(
                "SRS retention bonuses cannot be negative.");
        }
    }

    private static T Read<T>(
        IReadOnlyDictionary<string, string> values,
        string key,
        T defaultValue)
        where T : IParsable<T>
    {
        if (!values.TryGetValue(key, out var rawValue))
            return defaultValue;

        if (T.TryParse(rawValue, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new InvalidOperationException(
            $"System configuration '{key}' has invalid value '{rawValue}'.");
    }
}
