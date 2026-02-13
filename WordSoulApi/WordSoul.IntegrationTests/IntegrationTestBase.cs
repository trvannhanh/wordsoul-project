
using Microsoft.EntityFrameworkCore.Storage;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services.SRS;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;
using WordSoul.Infrastructure.Persistence.Repositories;
using WordSoul.IntegrationTests.Fixtures;

namespace WordSoul.IntegrationTests
{
    /// <summary>
    /// Base class for all integration tests
    /// Provides database context and service setup
    /// </summary>
    public class IntegrationTestBase : IDisposable
    {
        protected readonly WordSoulDbContext _context;
        protected readonly TestDataBuilder _dataBuilder;
        protected readonly SRSAlgorithm _srsAlgorithm;
        protected readonly IUnitOfWork _unitOfWork;

        public IntegrationTestBase()
        {
            // Create in-memory database
            _context = TestDbContextFactory.Create();
            _dataBuilder = new TestDataBuilder(_context);

            // Create algorithm
            _srsAlgorithm = new SRSAlgorithm();

            // Create UnitOfWork with real repositories
            _unitOfWork = CreateUnitOfWork();
        }

        private IUnitOfWork CreateUnitOfWork()
        {
            // Mock UnitOfWork - tạo wrapper cho repositories
            // (Bạn có thể dùng Moq hoặc tạo class thật)

            return new TestUnitOfWork(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    /// <summary>
    /// Test implementation of IUnitOfWork
    /// Uses real repositories with in-memory database
    /// </summary>
    public class TestUnitOfWork : IUnitOfWork
    {
        private readonly WordSoulDbContext _context;

        public TestUnitOfWork(WordSoulDbContext context)
        {
            _context = context;

            // Initialize repositories
            User = new UserRepository(_context);
            Vocabulary = new VocabularyRepository(_context);
            UserVocabularyProgress = new UserVocabularyProgressRepository(_context);
            VocabularyReviewHistory = new VocabularyReviewHistoryRepository(_context);
            AnswerRecord = new AnswerRecordRepository(_context);
            LearningSession = new LearningSessionRepository(_context);
            SessionVocabulary = new SessionVocabularyRepository(_context);
            VocabularySet = new VocabularySetRepository(_context);
            UserVocabularySet = new UserVocabularySetRepository(_context);
            UserOwnedPet = new UserOwnedPetRepository(_context);
            UserAchievement = new UserAchievementRepository(_context);
            SetVocabulary = new SetVocabularyRepository(_context);
            SetRewardPet = new SetRewardPetRepository(_context);
            Pet = new PetRepository(_context);
            Notification = new NotificationRepository(_context);
            Item = new ItemRepository(_context);
            Auth = new AuthRepository(_context);
            ActivityLog = new ActivityLogRepository(_context);
            Achievement = new AchievementRepository(_context);
            // ... add other repositories as needed
        }

        public IUserRepository User { get; }
        public IVocabularyRepository Vocabulary { get; }
        public IUserVocabularyProgressRepository UserVocabularyProgress { get; }
        public IVocabularyReviewHistoryRepository VocabularyReviewHistory { get; }
        public IAnswerRecordRepository AnswerRecord { get; }
        public ILearningSessionRepository LearningSession { get; }
        public ISessionVocabularyRepository SessionVocabulary { get; }
        public IVocabularySetRepository VocabularySet { get; }
        public IUserVocabularySetRepository UserVocabularySet { get; }
        public IUserOwnedPetRepository UserOwnedPet { get; }
        public IUserAchievementRepository UserAchievement { get; }
        public ISetVocabularyRepository SetVocabulary { get; }
        public ISetRewardPetRepository SetRewardPet { get; }
        public IPetRepository Pet { get; }
        public INotificationRepository Notification { get; }
        public IItemRepository Item { get; }
        public IAuthRepository Auth { get; }
        public IActivityLogRepository ActivityLog { get; }
        public IAchievementRepository Achievement { get; }
        // ... other repositories

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
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
