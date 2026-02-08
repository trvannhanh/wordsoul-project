

using Microsoft.EntityFrameworkCore.Storage;
using WordSoul.Application.Interfaces.Repositories;

namespace WordSoul.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAchievementRepository Achievement { get; }
        IActivityLogRepository ActivityLog { get; }
        IAnswerRecordRepository AnswerRecord { get; }
        IAuthRepository Auth { get; }
        IItemRepository Item { get; }
        ILearningSessionRepository LearningSession { get; }
        INotificationRepository Notification { get; }
        IPetRepository Pet { get; }
        ISetRewardPetRepository SetRewardPet { get; }
        ISessionVocabularyRepository SessionVocabulary { get; }
        IUserRepository User { get; }
        IUserAchievementRepository UserAchievement { get; }
        IUserVocabularyProgressRepository UserVocabularyProgress { get; }
        IVocabularyRepository Vocabulary { get; }
        IVocabularySetRepository VocabularySet { get; }
        ISetVocabularyRepository SetVocabulary { get; }
        IUserVocabularySetRepository UserVocabularySet { get; }
        IUserOwnedPetRepository UserOwnedPet { get; }
        IVocabularyReviewHistoryRepository VocabularyReviewHistory { get; }
        

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    }
}
