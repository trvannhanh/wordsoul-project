using Microsoft.EntityFrameworkCore.Storage;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Infrastructure.Persistence.Repositories;


namespace WordSoul.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WordSoulDbContext _context;
        private IAchievementRepository? _achievement;
        private IActivityLogRepository? _activityLog;
        private IAnswerRecordRepository? _answerRecord;
        private IAuthRepository? _auth;
        private IItemRepository? _item;
        private ILearningSessionRepository? _learningSession;
        private INotificationRepository? _notification;
        private IPetRepository? _pet;
        private ISessionVocabularyRepository? _sessionVocabulary;
        private ISetRewardPetRepository? _setRewardPet;
        private ISetVocabularyRepository? _setVocabulary;
        private IUserAchievementRepository? _userAchievement;
        private IUserOwnedPetRepository? _userOwnedPet;
        private IUserRepository? _user;
        private IUserVocabularyProgressRepository? _userVocabularyProgress;
        private IUserVocabularySetRepository? _userVocabularySet;
        private IVocabularyRepository? _vocabulary;
        private IVocabularySetRepository? _vocabularySet;


        public UnitOfWork(WordSoulDbContext context)
        {
            _context = context;
        }

        public IAchievementRepository Achievement =>
            _achievement ??= new AchievementRepository(_context);
        public IActivityLogRepository ActivityLog =>
            _activityLog ??= new ActivityLogRepository(_context);
        public IAnswerRecordRepository AnswerRecord =>
            _answerRecord ??= new AnswerRecordRepository(_context);
        public IAuthRepository Auth =>
            _auth ??= new AuthRepository(_context);
        public IItemRepository Item =>
            _item ??= new ItemRepository(_context);
        public ILearningSessionRepository LearningSession =>
            _learningSession ??= new LearningSessionRepository(_context);
        public INotificationRepository Notification =>
            _notification ??= new NotificationRepository(_context);
        public IPetRepository Pet =>
            _pet ??= new PetRepository(_context);
        public ISessionVocabularyRepository SessionVocabulary =>
            _sessionVocabulary ??= new SessionVocabularyRepository(_context);
        public ISetRewardPetRepository SetRewardPet =>
            _setRewardPet ??= new SetRewardPetRepository(_context);
        public ISetVocabularyRepository SetVocabulary =>
            _setVocabulary ??= new SetVocabularyRepository(_context);
        public IUserRepository User => 
            _user ??= new UserRepository(_context);
        public IUserAchievementRepository UserAchievement => 
            _userAchievement ??= new UserAchievementRepository(_context);
        public IUserVocabularyProgressRepository UserVocabularyProgress => 
            _userVocabularyProgress ??= new UserVocabularyProgressRepository(_context);
        public IVocabularyRepository Vocabulary => 
            _vocabulary ??= new VocabularyRepository(_context);
        public IVocabularySetRepository VocabularySet => 
            _vocabularySet ??= new VocabularySetRepository(_context);

        public IUserVocabularySetRepository UserVocabularySet => 
            _userVocabularySet ??= new UserVocabularySetRepository(_context);
        public IUserOwnedPetRepository UserOwnedPet =>
            _userOwnedPet ??= new UserOwnedPetRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return await _context.Database.BeginTransactionAsync(ct);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
