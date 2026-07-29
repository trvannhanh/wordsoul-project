using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.InitialRecall;

public sealed record InitialRecallGradeResult(
    int Grade,
    int PolicyVersion,
    ReviewGradeReason Reason);
