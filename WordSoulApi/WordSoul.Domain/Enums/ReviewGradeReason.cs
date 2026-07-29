namespace WordSoul.Domain.Enums;

// Values are persisted in review outcomes. Never reorder or reuse them.
public enum ReviewGradeReason
{
    LegacyPolicy = 0,
    FastUnaidedRecall = 1,
    HesitantUnaidedRecall = 2,
    DifficultOrAssistedRecall = 3,
    FailedInitialRecall = 4
}
