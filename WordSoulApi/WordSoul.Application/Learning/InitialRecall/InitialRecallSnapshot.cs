using WordSoul.Domain.Entities;

namespace WordSoul.Application.Learning.InitialRecall;

public sealed record InitialRecallSnapshot(
    AnswerRecord AnswerRecord,
    bool IsCorrect,
    int Grade,
    DateTime CapturedAt);
