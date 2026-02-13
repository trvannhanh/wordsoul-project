using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.IntegrationTests.Fixtures
{
    /// <summary>
    /// Helper class to build test entities
    /// </summary>
    public class TestDataBuilder
    {
        private readonly WordSoulDbContext _context;

        public TestDataBuilder(WordSoulDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo user test
        /// </summary>
        public async Task<User> CreateUserAsync(string username = "testuser")
        {
            var user = new User
            {
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = "hash123",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Tạo vocabulary test
        /// </summary>
        public async Task<Vocabulary> CreateVocabularyAsync(
            string word = "apple",
            string meaning = "táo")
        {
            var vocab = new Vocabulary
            {
                Word = word,
                Meaning = meaning,
                Pronunciation = "/ˈæpl/",
                PartOfSpeech = PartOfSpeech.Noun,
                CEFRLevel = CEFRLevel.A1,
                Description = "A fruit"
            };

            _context.Vocabularies.Add(vocab);
            await _context.SaveChangesAsync();

            return vocab;
        }

        /// <summary>
        /// Tạo UserVocabularyProgress với SRS params
        /// </summary>
        public async Task<UserVocabularyProgress> CreateProgressAsync(
            int userId,
            int vocabularyId,
            double easeFactor = 2.5,
            int interval = 1,
            int repetition = 0)
        {
            var progress = new UserVocabularyProgress
            {
                UserId = userId,
                VocabularyId = vocabularyId,
                EasinessFactor = easeFactor,
                Interval = interval,
                Repetition = repetition,
                NextReviewTime = DateTime.UtcNow.AddDays(interval),
                CorrectAttempt = 0,
                TotalAttempt = 0,
                CorrectCount = 0,
                WrongCount = 0,
                RetentionScore = 0,
                MemoryState = "New",
                FirstLearnedAt = DateTime.UtcNow
            };

            _context.UserVocabularyProgresses.Add(progress);
            await _context.SaveChangesAsync();

            return progress;
        }

        /// <summary>
        /// Tạo LearningSession
        /// </summary>
        public async Task<LearningSession> CreateReviewSessionAsync(
            int userId,
            List<Vocabulary> vocabularies)
        {
            var session = new LearningSession
            {
                UserId = userId,
                Type = SessionType.Review,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                SessionVocabularies = vocabularies.Select((v, index) => new SessionVocabulary
                {
                    VocabularyId = v.Id,
                    Order = index + 1,
                    CurrentLevel = 0,
                    IsCompleted = false
                }).ToList()
            };

            _context.LearningSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }
    }
}
