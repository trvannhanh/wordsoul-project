using WordSoul.Application.DTOs.SRS;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Learning.ReviewOutcome;

public sealed record ReviewOutcomeResult(
    VocabularyReviewHistory History,
    SRSUpdateResult SrsResult);
