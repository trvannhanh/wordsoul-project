using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Gym;
using WordSoul.Application.DTOs.QuizQuestion;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence
{
    /// <summary>
    /// Core Battle Engine: PvE Gym Battle (và tương lai PvP).
    /// Đặt trong Infrastructure vì truy cập trực tiếp DbContext để query linh hoạt.
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly WordSoulDbContext _db;
        private readonly ILogger<BattleService> _logger;

        public BattleService(WordSoulDbContext db, ILogger<BattleService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // START GYM BATTLE
        // ═══════════════════════════════════════════════════════════════

        public async Task<StartBattleResponseDto> StartGymBattleAsync(
            int userId, int gymId, CancellationToken ct = default)
        {
            // 1. Load Gym
            var gym = await _db.GymLeaders
                .FirstOrDefaultAsync(gl => gl.Id == gymId, ct)
                ?? throw new KeyNotFoundException($"Gym {gymId} not found.");

            // 2. Kiểm tra UserGymProgress
            var progress = await _db.UserGymProgresses
                .FirstOrDefaultAsync(ugp => ugp.UserId == userId && ugp.GymLeaderId == gymId, ct);

            if (progress == null || progress.Status == GymStatus.Locked)
                throw new InvalidOperationException($"Gym '{gym.Name}' is still locked. Meet the unlock requirements first.");

            // 3. Kiểm tra cooldown
            if (progress.IsOnCooldown(gym.CooldownHours))
            {
                var cooldownEnd = progress.CooldownEndsAt(gym.CooldownHours);
                throw new InvalidOperationException(
                    $"Gym '{gym.Name}' is on cooldown until {cooldownEnd:yyyy-MM-dd HH:mm} UTC.");
            }

            // 4. Chọn câu hỏi (vocab) theo theme + CEFR
            var vocabs = await SelectBattleVocabsAsync(userId, gym, ct);

            if (vocabs.Count == 0)
                throw new InvalidOperationException(
                    $"Not enough vocabulary for this battle. Keep learning {gym.Theme} words!");

            // 5. Tạo BattleSession
            var session = new BattleSession
            {
                ChallengerUserId = userId,
                OpponentUserId = null,          // PvE
                GymLeaderId = gymId,
                Type = BattleType.GymBattle,
                Status = BattleStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                TotalQuestions = vocabs.Count
            };

            _db.BattleSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "User {UserId} started GymBattle vs Gym {GymId} ({GymName}), Session {SessionId}",
                userId, gymId, gym.Name, session.Id);

            // 6. Tạo QuizQuestions từ vocab list
            var allWords = vocabs.Select(v => v.Word ?? "").Where(w => w.Length > 0).ToList();
            var questions = vocabs.Select((v, idx) =>
                BuildQuizQuestion(v, idx, allWords)).ToList();

            return new StartBattleResponseDto
            {
                BattleSessionId = session.Id,
                GymLeaderId = gym.Id,
                GymLeaderName = gym.Name,
                GymLeaderTitle = gym.Title,
                GymLeaderAvatarUrl = gym.AvatarUrl,
                TotalQuestions = questions.Count,
                PassRatePercent = gym.PassRatePercent,
                Questions = questions
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // SUBMIT BATTLE
        // ═══════════════════════════════════════════════════════════════

        public async Task<BattleResultDto> SubmitBattleAsync(
            int userId, int battleSessionId,
            SubmitBattleRequestDto request,
            CancellationToken ct = default)
        {
            // 1. Load session
            var session = await _db.BattleSessions
                .Include(bs => bs.GymLeader)
                .FirstOrDefaultAsync(bs => bs.Id == battleSessionId && bs.ChallengerUserId == userId, ct)
                ?? throw new KeyNotFoundException($"BattleSession {battleSessionId} not found for user {userId}.");

            if (session.Status == BattleStatus.Completed)
                throw new InvalidOperationException("This battle session is already completed.");

            var gym = session.GymLeader!;

            // 2. Load vocab entities để chấm điểm
            var vocabIds = request.Answers.Select(a => a.VocabularyId).ToList();
            var vocabs = await _db.Vocabularies
                .AsNoTracking()
                .Where(v => vocabIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, ct);

            // 3. Chấm điểm từng câu
            var answerResults = new List<BattleAnswerResultDto>();
            int correctCount = 0;

            foreach (var answer in request.Answers)
            {
                var vocab = vocabs.GetValueOrDefault(answer.VocabularyId);
                if (vocab == null) continue;

                bool isCorrect = string.Equals(
                    answer.Answer.Trim(),
                    vocab.Word?.Trim(),
                    StringComparison.OrdinalIgnoreCase);

                if (isCorrect) correctCount++;

                answerResults.Add(new BattleAnswerResultDto
                {
                    VocabularyId = vocab.Id,
                    Word = vocab.Word ?? "",
                    Meaning = vocab.Meaning ?? "",
                    UserAnswer = answer.Answer,
                    IsCorrect = isCorrect,
                    QuestionOrder = answer.QuestionOrder
                });
            }

            // 4. Tính điểm %
            int totalQ = request.Answers.Count > 0 ? request.Answers.Count : session.TotalQuestions;
            int scorePercent = totalQ == 0 ? 0 : (int)Math.Round((double)correctCount / totalQ * 100);
            bool isVictory = scorePercent >= gym.PassRatePercent;

            // 5. Cập nhật BattleSession
            session.ChallengerCorrect = correctCount;
            session.TotalQuestions = totalQ;
            session.ChallengerWon = isVictory;
            session.Status = BattleStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;

            // 6. Cập nhật UserGymProgress
            var progress = await _db.UserGymProgresses
                .FirstOrDefaultAsync(ugp => ugp.UserId == userId && ugp.GymLeaderId == gym.Id, ct)!;

            if (progress == null)
            {
                progress = new UserGymProgress { UserId = userId, GymLeaderId = gym.Id };
                _db.UserGymProgresses.Add(progress);
            }

            progress.TotalAttempts++;
            progress.LastAttemptAt = DateTime.UtcNow;
            if (scorePercent > progress.BestScore) progress.BestScore = scorePercent;

            int xpEarned = 0;
            bool badgeEarned = false;
            string? badgeName = null;
            string? badgeImageUrl = null;

            if (isVictory && progress.Status != GymStatus.Defeated)
            {
                progress.Status = GymStatus.Defeated;
                progress.DefeatedAt = DateTime.UtcNow;

                // 7a. Phát XP reward
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (user != null)
                {
                    user.XP += gym.XpReward;
                    xpEarned = gym.XpReward;
                }

                // 7b. Cấp Badge Achievement
                if (gym.BadgeAchievementId.HasValue)
                {
                    var achievement = await _db.Achievements
                        .FirstOrDefaultAsync(a => a.Id == gym.BadgeAchievementId.Value, ct);

                    if (achievement != null)
                    {
                        var existing = await _db.UserAchievements
                            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id, ct);

                        if (existing == null)
                        {
                            _db.UserAchievements.Add(new UserAchievement
                            {
                                UserId = userId,
                                AchievementId = achievement.Id,
                                ProgressValue = 1,
                                IsCompleted = true,
                                CompletedAt = DateTime.UtcNow
                            });
                            badgeEarned = true;
                            badgeName = achievement.Name;
                            badgeImageUrl = gym.BadgeImageUrl;
                        }
                    }
                }

                _logger.LogInformation(
                    "User {UserId} DEFEATED Gym {GymId} ({GymName})! Score: {Score}%, XP: +{Xp}",
                    userId, gym.Id, gym.Name, scorePercent, xpEarned);
            }
            else if (!isVictory)
            {
                _logger.LogInformation(
                    "User {UserId} LOST vs Gym {GymId} ({GymName}). Score: {Score}%",
                    userId, gym.Id, gym.Name, scorePercent);
            }

            await _db.SaveChangesAsync(ct);

            return new BattleResultDto
            {
                BattleSessionId = session.Id,
                GymLeaderId = gym.Id,
                GymLeaderName = gym.Name,
                IsVictory = isVictory,
                CorrectAnswers = correctCount,
                TotalQuestions = totalQ,
                ScorePercent = scorePercent,
                PassRatePercent = gym.PassRatePercent,
                XpEarned = xpEarned,
                BadgeEarned = badgeEarned,
                BadgeName = badgeName,
                BadgeImageUrl = badgeImageUrl,
                IsOnCooldown = !isVictory && progress.IsOnCooldown(gym.CooldownHours),
                CooldownEndsAt = !isVictory ? progress.CooldownEndsAt(gym.CooldownHours) : null,
                NewGymStatus = progress.Status,
                BestScore = progress.BestScore,
                AnswerResults = answerResults
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Chọn vocab cho battle theo thứ tự ưu tiên:
        ///   1. Từ user đang học (MemoryState = Learning / Review) theo theme + CEFR
        ///   2. Fallback: từ "New" hoặc "Mastered" của cùng theme + CEFR để đủ số lượng
        /// </summary>
        private async Task<List<Vocabulary>> SelectBattleVocabsAsync(
            int userId, GymLeader gym, CancellationToken ct)
        {
            int needed = gym.QuestionCount;

            // Priority 1: từ user đã học (Learning / Review / Mastered) theo theme + CEFR
            var priorityStates = new[] { "Learning", "Review", "Mastered" };

            var priorityVocabs = await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp =>
                    uvp.UserId == userId &&
                    priorityStates.Contains(uvp.MemoryState) &&
                    uvp.Vocabulary!.CEFRLevel == gym.RequiredCefrLevel)
                .Include(uvp => uvp.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())           // random shuffle
                .Take(needed)
                .Select(uvp => uvp.Vocabulary!)
                .ToListAsync(ct);

            if (priorityVocabs.Count >= needed)
                return priorityVocabs;

            // Fallback: lấy thêm vocab cùng theme + CEFR (chưa học hoặc đã học)
            var alreadySelectedIds = priorityVocabs.Select(v => v.Id).ToHashSet();
            int remaining = needed - priorityVocabs.Count;

            var fallbackVocabs = await _db.Vocabularies
                .AsNoTracking()
                .Where(v =>
                    v.CEFRLevel == gym.RequiredCefrLevel &&
                    !alreadySelectedIds.Contains(v.Id))
                .OrderBy(_ => Guid.NewGuid())
                .Take(remaining)
                .ToListAsync(ct);

            priorityVocabs.AddRange(fallbackVocabs);

            // Shuffle cuối để đảm bảo random thứ tự
            return priorityVocabs.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// Tạo QuizQuestionDto từ Vocabulary cho battle.
        /// Battle sử dụng MultipleChoice và FillInBlank (không Flashcard, không Listening).
        /// </summary>
        private static QuizQuestionDto BuildQuizQuestion(
            Vocabulary vocab, int index, List<string> allWords)
        {
            // Luân phiên: chẵn = MultipleChoice, lẻ = FillInBlank
            var questionType = index % 2 == 0 ? QuestionType.MultipleChoice : QuestionType.FillInBlank;

            string? questionPrompt = questionType switch
            {
                QuestionType.MultipleChoice => vocab.Meaning,
                QuestionType.FillInBlank when
                    !string.IsNullOrWhiteSpace(vocab.Description) &&
                    vocab.Word != null &&
                    vocab.Description.Contains(vocab.Word, StringComparison.OrdinalIgnoreCase)
                    => BuildBlankSentence(vocab.Description, vocab.Word!),
                _ => null
            };

            List<string>? options = null;
            if (questionType == QuestionType.MultipleChoice)
            {
                var distractors = allWords
                    .Where(w => !string.Equals(w, vocab.Word, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                while (distractors.Count < 3) distractors.Add("—");
                options = distractors.Append(vocab.Word ?? "").OrderBy(_ => Guid.NewGuid()).ToList();
            }

            return new QuizQuestionDto
            {
                VocabularyId = vocab.Id,
                Word = vocab.Word,
                Meaning = vocab.Meaning,
                Pronunciation = vocab.Pronunciation,
                PronunciationUrl = vocab.PronunciationUrl,
                ImageUrl = vocab.ImageUrl,
                Description = vocab.Description,
                PartOfSpeech = vocab.PartOfSpeech?.ToString(),
                CEFRLevel = vocab.CEFRLevel?.ToString(),
                QuestionType = questionType,
                Options = options,
                IsRetry = false,
                QuestionPrompt = questionPrompt
            };
        }

        private static string BuildBlankSentence(string description, string word)
        {
            var pattern = System.Text.RegularExpressions.Regex.Escape(word);
            return System.Text.RegularExpressions.Regex.Replace(
                description, pattern, "___",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
