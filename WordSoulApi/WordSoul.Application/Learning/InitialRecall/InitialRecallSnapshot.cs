using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.InitialRecall;

public sealed record InitialRecallSnapshot(
    AnswerRecord AnswerRecord,
    bool IsCorrect,
    int Grade,
    int GradingPolicyVersion,
    ReviewGradeReason GradeReason,
    DateTime CapturedAt);
