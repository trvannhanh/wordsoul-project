using Microsoft.Extensions.Logging;
using System;
using WordSoul.Application.Common;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.LearningSession;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.DTOs.QuizQuestion;
using WordSoul.Application.DTOs.SRS;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    /// <summary>
    /// Service quản lý LearningSession: tạo session, lấy câu hỏi, submit answer, complete session.
    /// Sử dụng IUnitOfWork để gọi repository và commit transaction.
    /// </summary>
    public class LearningSessionService : ILearningSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<LearningSessionService> _logger;
        private readonly IUserOwnedPetService _userOwnedPetService;
        private readonly IUserVocabularyProgressService _userVocabularyProgressService;
        private readonly IActivityLogService _activityLogService;
        private readonly ISetRewardPetService _setRewardPetService;
        private readonly ISRSService _srsService;
        private readonly IDailyQuestService _dailyQuestService;
        private readonly ITimeProvider _timeProvider;
        private readonly IPetBuffService _petBuffService;
        private readonly IGymLeaderService _gymLeaderService;

        /// <summary>
        /// Khởi tạo LearningSessionService.
        /// </summary>
        public LearningSessionService(
            IUnitOfWork uow,
            ILogger<LearningSessionService> logger,
            IUserOwnedPetService userOwnedPetService,
            IUserVocabularyProgressService userVocabularyProgressService,
            IActivityLogService activityLogService,
            ISetRewardPetService setRewardPetService,
            ISRSService srsService,
            IDailyQuestService dailyQuestService,
            IPetBuffService petBuffService,
            ITimeProvider timeProvider,
            IGymLeaderService gymLeaderService)
        {
            _uow = uow;
            _logger = logger;
            _userOwnedPetService = userOwnedPetService;
            _userVocabularyProgressService = userVocabularyProgressService;
            _activityLogService = activityLogService;
            _setRewardPetService = setRewardPetService;
            _srsService = srsService;
            _dailyQuestService = dailyQuestService;
            _timeProvider = timeProvider;
            _petBuffService = petBuffService;
            _gymLeaderService = gymLeaderService;
        }

        // ------------------------------------CREATE-----------------------------------------

        /// <summary>
        /// Tạo một phiên học mới cho user dựa trên VocabularySet đã chọn.
        /// Nếu user đã có session chưa hoàn thành sẽ trả về session hiện tại.
        /// </summary>
        public async Task<LearningSessionDto> CreateLearningSessionAsync(
            int userId,
            int setId,
            int wordCount = 5,
            CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (setId <= 0) throw new ArgumentException("VocabularySetId must be greater than zero.", nameof(setId));
            if (wordCount <= 0) throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var existingSession = await _uow.LearningSession
                .GetExistingLearningSessionUnCompletedForUserAsync(userId, setId, ct);

            if (existingSession != null)
            {
                _logger.LogInformation("User {UserId} already has an existing uncompleted learning session for set {SetId}", userId, setId);

                var correctAnswerNumber = await _uow.AnswerRecord.GetCorrectAnswerRecordNumberFromSession(existingSession.Id, ct);

                return new LearningSessionDto
                {
                    Id = existingSession.Id,
                    IsCompleted = existingSession.IsCompleted,
                    VocabularyIds = existingSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                    PetId = existingSession.PetId,
                    CatchRate = existingSession.CatchRate,
                    CurrentCorrectAnswered = correctAnswerNumber,
                    BuffPetId = existingSession?.BuffPetId,
                    BuffName = existingSession?.BuffName,
                    BuffDescription = existingSession?.BuffDescription,
                    BuffIcon = existingSession?.BuffIcon,

                    PetXpMultiplier = existingSession?.PetXpMultiplier ?? 1.0,
                    PetCatchBonus = existingSession?.PetCatchBonus ?? 0,
                    PetHintShield = existingSession?.PetHintShield ?? false,
                    PetReducePenalty = existingSession?.PetReducePenalty ?? false,
                };
            }

            var exists = await _uow.UserVocabularySet.GetUserVocabularySetAsync(userId, setId, ct);
            if (exists == null)
            {
                _logger.LogWarning("User {UserId} doesn't own VocabularySet {SetId}", userId, setId);
                throw new InvalidOperationException("VocabularySet doesn't exist for this user");
            }

            var vocabularies = await _uow.SetVocabulary.GetUnlearnedVocabulariesFromSetAsync(userId, setId, wordCount, ct);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unlearned vocabularies for user {UserId} in set {SetId}", userId, setId);
                throw new InvalidOperationException("No unlearned vocabularies available in this set");
            }

            var randomPet = await _setRewardPetService.GetRandomPetBySetIdAsync(setId, exists.TotalCompletedSession, ct);
            if (randomPet == null)
            {
                _logger.LogInformation("No pets in set {SetId}", setId);
                throw new InvalidOperationException("No pets available in this set");
            }

            var petCatchRate = GetPetCatchRate(randomPet.Rarity);


            return await CreateSessionAsync(userId, setId, SessionType.Learning, vocabularies, randomPet.Id, petCatchRate, ct);
        }

        /// <summary>
        /// Tạo một phiên học ôn tập mới cho user.
        /// </summary>
        public async Task<LearningSessionDto> CreateReviewingSessionAsync(
            int userId, // Id người dùng
            int wordCount = 5, // Số từ trong phiên ôn tập
            CancellationToken ct = default)
        {
            // Validation
            if (userId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (wordCount <= 0) throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            // Kiểm tra session ôn tập chưa hoàn thành hiện có
            var existingSession = await _uow.LearningSession
                .GetExistingReviewSessionUnCompletedForUserAsync(userId, ct);

            // Nếu có session chưa hoàn thành, trả về session đó
            if (existingSession != null)
            {
                _logger.LogInformation("User {UserId} already has an existing uncompleted review session", userId);

                // Trả về session hiện tại
                var correctAnswerNumber = await _uow.AnswerRecord.GetCorrectAnswerRecordNumberFromSession(existingSession.Id, ct);
                return new LearningSessionDto
                {
                    Id = existingSession.Id,
                    IsCompleted = existingSession.IsCompleted,
                    VocabularyIds = existingSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                    PetId = existingSession.PetId,
                    CatchRate = existingSession.CatchRate,
                    CurrentCorrectAnswered = correctAnswerNumber
                };
            }

            // có thêm biến đầu vào setID để lấy từ vựng từ bộ cụ thể...

            var dueDtos = await _srsService.GetDueVocabulariesAsync(userId, wordCount, ct);

            if (!dueDtos.Any())
            {
                _logger.LogInformation("No due vocabularies for user {UserId}", userId);
                throw new InvalidOperationException("No vocabularies due for review");
            }

            // Lấy Vocabulary entities từ DB
            var vocabIds = dueDtos.Select(d => d.VocabularyId).ToList();

            var vocabularies = await _uow.Vocabulary
                .GetVocabulariesByIdsAsync(vocabIds, ct);

            return await CreateSessionAsync(userId, null, SessionType.Review, vocabularies, null, null, ct);
        }

        /// <summary>
        /// Helper: tạo session và persist qua UnitOfWork.
        /// </summary>
        private async Task<LearningSessionDto> CreateSessionAsync(
            int userId, // Id người dùng
            int? setId, // Id bộ từ vựng (nếu có)
            SessionType type, // Loại session (Learning/Review)
            IEnumerable<Vocabulary> vocabularies, // Danh sách từ vựng trong session
            int? petId, // Id pet (nếu có)
            double? catchRate, // Tỷ lệ bắt pet (nếu có)
            CancellationToken ct)
        {
            var petBuff = await _petBuffService.GetActivePetBuffAsync(userId, ct);


            // Tạo đối tượng LearningSession mới
            var session = new LearningSession
            {
                UserId = userId,
                VocabularySetId = setId,
                Type = type,
                StartTime = _timeProvider.UtcNow,
                IsCompleted = false,
                SessionVocabularies = vocabularies.Select((v, index) => new SessionVocabulary
                {
                    VocabularyId = v.Id,
                    Order = index + 1,
                    CurrentLevel = 0,
                    IsCompleted = false
                }).ToList(),
                PetId = petId,
                BuffPetId = petBuff?.PetId,
                CatchRate = catchRate,

                BuffName = petBuff?.BuffName,
                BuffDescription = petBuff?.BuffDescription,
                BuffIcon = petBuff?.BuffIcon,


                PetXpMultiplier = petBuff?.XpMultiplier ?? 1.0,
                PetCatchBonus = petBuff?.CatchRateBonus ?? 0,
                PetHintShield = petBuff?.HasHintShield ?? false,
                PetReducePenalty = petBuff?.ReducePenalty ?? false,

            };

            session.CatchRate += session.PetCatchBonus;

            var savedSession = await _uow.LearningSession.CreateLearningSessionAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            await _activityLogService.TrackStartLearningSessionAsync(userId, savedSession.Id, ct);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                PetId = savedSession.PetId,
                BuffPetId = savedSession.BuffPetId,
                CatchRate = catchRate,
                CurrentCorrectAnswered = 0,
                BuffName = savedSession.BuffName,
                BuffDescription = savedSession.BuffDescription,
                BuffIcon = savedSession.BuffIcon,

                PetXpMultiplier = savedSession.PetXpMultiplier,
                PetCatchBonus = savedSession.PetCatchBonus,
                PetHintShield = savedSession.PetHintShield,
                PetReducePenalty = savedSession.PetReducePenalty,
            };
        }

        //------------------------------------READ-------------------------------------------

        /// <summary>
        /// Lấy danh sách câu hỏi quiz cho một phiên học cụ thể.
        /// Nếu session đã complete thì trả về rỗng.
        /// </summary>
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId, CancellationToken ct = default)
        {
            // Validation
            var session = await _uow.LearningSession.GetLearningSessionByIdAsync(sessionId, ct);
            if (session?.IsCompleted == true) return Enumerable.Empty<QuizQuestionDto>();

            // Lấy tất cả từ vựng trong session
            var sessionVocabs = await _uow.SessionVocabulary.GetSessionVocabulariesBySessionIdAsync(sessionId, ct);
            // Lọc từ vựng chưa hoàn thành
            var incompleteVocabs = sessionVocabs.Where(sv => !sv.IsCompleted).ToList();
            // Nếu tất cả từ vựng đã hoàn thành, trả về rỗng
            if (!incompleteVocabs.Any())
            {
                _logger.LogInformation("All vocabularies completed for session {SessionId}", sessionId);
                return Enumerable.Empty<QuizQuestionDto>();
            }

            // Tạo danh sách câu hỏi quiz từ từ vựng chưa hoàn thành
            var allWords = sessionVocabs.Select(sv => sv.Vocabulary!.Word).ToList();

            var questions = new List<QuizQuestionDto>();

            // Map level to question type
            var levelToType = new Dictionary<int, QuestionType>
            {
                {0, QuestionType.Flashcard},
                {1, QuestionType.FillInBlank},
                {2, QuestionType.MultipleChoice},
                {3, QuestionType.Listening}
            };

            // Tạo câu hỏi cho từng từ vựng chưa hoàn thành
            foreach (var sv in incompleteVocabs)
            {
                var vocab = sv.Vocabulary;
                if (vocab == null) continue;

                var questionType = levelToType[sv.CurrentLevel];
                questions.Add(CreateQuizQuestionDto(vocab, questionType, allWords, sv.CurrentLevel > 0));
            }

            return questions.OrderBy(q => sessionVocabs.First(sv => sv.VocabularyId == q.VocabularyId).Order);
        }

        // Helper: tạo QuizQuestionDto từ Vocabulary và QuestionType
        private QuizQuestionDto CreateQuizQuestionDto(Vocabulary vocab, QuestionType type, List<string> allWords, bool isRetry)
        {
            // ── Proposal A: MCQ ──────────────────────────────────────────────────────────
            // Đối với MultipleChoice: QuestionPrompt = Meaning của từ (người dùng xem nghĩa → chọn từ đúng)
            // Options vẫn là danh sách các Word, đảm bảo luôn có đủ 3 distractors.
            // ── Proposal B: FillInBlank ──────────────────────────────────────────────────
            // Đối với FillInBlank: QuestionPrompt = câu ví dụ từ Description với từ bị thay bằng "___"
            // Nếu Description null hoặc không chứa từ → fallback về null (GameScreen sẽ dùng Meaning bình thường)

            string? questionPrompt = type switch
            {
                QuestionType.MultipleChoice => vocab.Meaning,

                QuestionType.FillInBlank when
                    !string.IsNullOrWhiteSpace(vocab.Description) &&
                    vocab.Description.Contains(vocab.Word, StringComparison.OrdinalIgnoreCase)
                    => BuildContextSentence(vocab.Description, vocab.Word),

                _ => null
            };

            // Đảm bảo MCQ luôn có đủ 3 distractor words (loại trừ từ đúng)
            List<string>? options = null;
            if (type == QuestionType.MultipleChoice)
            {
                var distractors = allWords
                    .Where(w => !string.Equals(w, vocab.Word, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                // Nếu không đủ 3 distractors từ session, thêm placeholder để đảm bảo UI không bị lỗi
                while (distractors.Count < 3)
                    distractors.Add($"—");

                options = distractors.Append(vocab.Word).OrderBy(_ => Guid.NewGuid()).ToList();
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
                PartOfSpeech = vocab.PartOfSpeech.ToString(),
                CEFRLevel = vocab.CEFRLevel.ToString(),
                QuestionType = type,
                Options = options,
                IsRetry = isRetry,
                QuestionPrompt = questionPrompt,
            };
        }

        /// <summary>
        /// Tạo câu ví dụ (context sentence) bằng cách thay thế từ cần học bằng "___".
        /// So sánh case-insensitive để tìm vị trí từ trong câu.
        /// </summary>
        private static string BuildContextSentence(string description, string word)
        {
            // Dùng regex để thay thế toàn bộ word boundary một cách an toàn
            var pattern = System.Text.RegularExpressions.Regex.Escape(word);
            return System.Text.RegularExpressions.Regex.Replace(
                description,
                pattern,
                "___",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        //------------------------------------UPDATE-----------------------------------------

        /// <summary>
        /// Hoàn thành session: cập nhật trạng thái, cộng XP/AP, grant pet, upgrade pet, update progress.
        /// Trả về DTO tuỳ theo SessionType.
        /// </summary>
        public async Task<object> CompleteSessionAsync(
            int userId,
            int sessionId,
            SessionType sessionType,
            CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (sessionId <= 0) throw new ArgumentException("SessionId must be greater than zero.", nameof(sessionId));

            var session = await _uow.LearningSession.GetLearningSessionByIdAsync(sessionId, ct);
            if (session == null || session.UserId != userId) throw new UnauthorizedAccessException("User does not have access to this session");
            if (session.IsCompleted) throw new InvalidOperationException("Session is already completed");

            var sessionVocabs = await _uow.SessionVocabulary.GetSessionVocabulariesBySessionIdAsync(sessionId, ct);
            if (sessionVocabs.Any(sv => !sv.IsCompleted)) throw new InvalidOperationException("Not all vocabularies are completed in this session");

            // Update session
            session.IsCompleted = true;
            session.EndTime = _timeProvider.UtcNow;
            await _uow.LearningSession.UpdateLearningSessionAsync(session, ct);

            await _activityLogService.TrackFinishLearningSessionAsync(userId, sessionId, ct);

            int baseXp = sessionType == SessionType.Learning ? 20 : 100;
            int xpEarned = (int)Math.Round(baseXp * session.PetXpMultiplier);


            int apEarned = sessionType == SessionType.Review ? 3 : 0;

            // Learning-specific updates
            bool isPetRewardGranted = false;
            bool isPetAlreadyOwned = false;
            int petId = 0;
            string? petName = null;
            string? petDescription = null;
            string? petImageUrl = null;
            string? petRarity = null;
            string? petType = null;

            if (sessionType == SessionType.Learning && session.VocabularySetId.HasValue)
            {
                var userVocabularySet = await _uow.UserVocabularySet.GetUserVocabularySetAsync(userId, session.VocabularySetId.Value, ct);
                if (userVocabularySet != null)
                {
                    userVocabularySet.TotalCompletedSession += 1;

                    var vocabularies = await _uow.SetVocabulary.GetUnlearnedVocabulariesFromSetAsync(userId, session.VocabularySetId.Value, 1, ct);
                    if (!vocabularies.Any()) userVocabularySet.IsCompleted = true;

                    await _uow.UserVocabularySet.UpdateUserVocabularySetAsync(userVocabularySet, ct);
                }
            }

            // Pet reward logic
            if (sessionType == SessionType.Learning && session.PetId.HasValue && session.CatchRate.HasValue)
            {
                if (!session.VocabularySetId.HasValue) throw new InvalidOperationException("VocabularySetId is required for learning session");

                var (alreadyOwned, isSuccess, bonusXp) = await _userOwnedPetService.GrantPetAsync(userId, session.PetId.Value, session.CatchRate.Value, ct);
                isPetRewardGranted = isSuccess;
                isPetAlreadyOwned = alreadyOwned;
                if (alreadyOwned) xpEarned += bonusXp;

                var pet = await _uow.Pet.GetPetByIdAsync(session.PetId.Value, ct);
                if (pet != null)
                {
                    petId = pet.Id;
                    petName = pet.Name;
                    petDescription = pet.Description;
                    petImageUrl = pet.ImageUrl;
                    petRarity = pet.Rarity.ToString();
                    petType = pet.Type.ToString();
                }
            }

            // Update user XP/AP
            var user = await _uow.User.GetUserByIdAsync(userId, ct);
            if (user == null) throw new InvalidOperationException("User not found");

            await _uow.User.UpdateUserXPAndAPAsync(userId, xpEarned, apEarned, ct);

            // Upgrade active pet (if any)
            var activePet = await _uow.UserOwnedPet.GetActivePetByUserIdAsync(userId, ct);
            var petUpgraded = new UpgradePetDto();
            if (activePet != null)
            {
                petUpgraded = await _userOwnedPetService.UpgradePetForUserAsync(userId, activePet.Id, 100, ct);
            }

            // Commit all changes done so far
            await _uow.SaveChangesAsync(ct);

            if (sessionType == SessionType.Learning)
            {
                await _dailyQuestService.UpdateQuestProgressAsync(
                    userId,
                    QuestType.Learn,
                    1,
                    null,
                    ct);
            }
            else
            {
                await _dailyQuestService.UpdateQuestProgressAsync(
                    userId,
                    QuestType.Review,
                    1,
                    null,
                    ct);
            }

            // ── Kiểm tra và mở khóa Gym Leader mới (fire-and-forget style, không block response) ──
            try
            {
                await _gymLeaderService.CheckAndUnlockGymsAsync(userId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Non-critical: CheckAndUnlockGymsAsync failed for user {UserId}", userId);
            }

            // Return DTO
            if (sessionType == SessionType.Learning)
            {
                return new CompleteLearningSessionResponseDto
                {
                    XpEarned = xpEarned,
                    IsPetRewardGranted = isPetRewardGranted,
                    IsPetAlreadyOwned = isPetAlreadyOwned,
                    PetId = petId,
                    PetName = petName,
                    Description = petDescription,
                    ImageUrl = petImageUrl,
                    PetRarity = petRarity,
                    PetType = petType,
                    UserPetId = petUpgraded.PetId,
                    UserPetExperience = petUpgraded.Experience,
                    UserPetLevel = petUpgraded.Level,
                    UserPetIsLevelUp = petUpgraded.IsLevelUp,
                    UserPetIsEvolved = petUpgraded.IsEvolved,
                    Message = isPetRewardGranted ? "Learning session completed! You caught a Pet!" : "Learning session completed! You earned XP!"
                };
            }
            else
            {
                return new CompleteReviewingSessionResponseDto
                {
                    XpEarned = xpEarned,
                    ApEarned = apEarned,
                    UserPetId = petUpgraded.PetId,
                    UserPetExperience = petUpgraded.Experience,
                    UserPetLevel = petUpgraded.Level,
                    UserPetIsLevelUp = petUpgraded.IsLevelUp,
                    UserPetIsEvolved = petUpgraded.IsEvolved,
                    Message = "Reviewing session completed! You earned XP and AP!"
                };
            }
        }

        /// <summary>
        /// Xử lý khi user gửi câu trả lời cho một câu hỏi trong session.
        /// Cập nhật AnswerRecord, session vocabulary, và tính level/progress.
        /// </summary>
        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(
            int userId,
            int sessionId,
            SubmitAnswerRequestDto request,
            CancellationToken ct = default)
        {
            //Validation
            if (request == null || request.VocabularyId <= 0)
                throw new ArgumentException("Invalid request data");

            var session = await _uow.LearningSession
                .GetExistingLearningSessionForUserAsync(userId, sessionId, ct)
                ?? throw new UnauthorizedAccessException();

            var sessionVocab = await _uow.SessionVocabulary
                .GetSessionVocabularyAsync(sessionId, request.VocabularyId, ct)
                ?? throw new KeyNotFoundException();

            var vocab = sessionVocab.Vocabulary
                ?? throw new KeyNotFoundException();

            // Kiểm tra câu trả lời
            var isCorrect = CheckAnswer(request, vocab);

            // Đảm bảo UserVocabularyProgress tồn tại
            await EnsureUserVocabularyProgressAsync(userId, vocab.Id, ct);

            var progress = await _uow.UserVocabularyProgress
            .GetUserVocabularyProgressAsync(userId, vocab.Id, ct);

            if (progress != null)
            {
                progress.TotalAttempt++;

                if (isCorrect)
                    progress.CorrectAttempt++;
            }

            // Lưu AnswerRecord
            await SaveAnswerRecordAsync(sessionId, vocab.Id, request, isCorrect, ct);

            // Cập nhật SessionVocabulary
            if (isCorrect)
            {
                sessionVocab.CurrentLevel++;
                sessionVocab.IsCompleted = sessionVocab.CurrentLevel >= 4;
            }
            else
            {
                sessionVocab.CurrentLevel = Math.Max(0, sessionVocab.CurrentLevel - 1);
                sessionVocab.IsCompleted = false;

                if (session.CatchRate.HasValue)
                {
                    if (!session.PetReducePenalty)
                    {
                        session.CatchRate -= 0.05;
                    }
                }    
                    
            }

            await _uow.SessionVocabulary.UpdateSessionVocabularyAsync(sessionVocab, ct);

         
            await _uow.SaveChangesAsync(ct);



            // Cập nhật SRS và lưu lịch sử ôn tập nếu là session ôn tập và từ đã hoàn thành
            if (session.Type == SessionType.Review && sessionVocab.IsCompleted)
            {
                // Lấy tất cả các lần thử của từ này trong session
                var allAttempts = await _uow.AnswerRecord
                    .GetAllAnswerRecordAttemptsForVocabInSession(sessionId, vocab.Id, ct);

                // Tính toán grade SM-2
                int grade = CalculateSm2Grade(
                    isCorrect: sessionVocab.IsCompleted,
                    attemptCount: allAttempts.Count,
                    avgResponseTime: allAttempts.Average(a => a.ResponseTimeSeconds),
                    totalHints: allAttempts.Sum(a => a.HintCount)
                );

                //cần xem lại phần tính accuracy, có thể chỉ tính cho lần thử cuối cùng hoặc tính theo công thức khác để phản ánh đúng hơn mức độ ghi nhớ của người dùng
                double accuracy = allAttempts.Count(a => a.IsCorrect) / (double)allAttempts.Count;

                await _dailyQuestService.UpdateQuestProgressAsync(
                    userId,
                    QuestType.Accuracy,
                    1,
                    accuracy,
                    ct);

                // Cập nhật SRS
                var srsResult = await _srsService.UpdateAfterReviewAsync(
                    userId, vocab.Id, grade, ct);

                _logger.LogInformation(
                    "Updated SRS for User {UserId}, Vocab {VocabId}: Grade={Grade}, NextReview={NextReview}",
                    userId, vocab.Id, grade, srsResult.NextReviewDate);


                // Lưu lịch sử ôn tập
                var reviewHistory = new VocabularyReviewHistory
                {
                    UserId = userId,
                    VocabularyId = sessionVocab.VocabularyId,
                    ReviewTime = _timeProvider.UtcNow,
                    IsCorrect = isCorrect,
                    ResponseTimeSeconds = request.ResponseTimeSeconds,
                    HintCount = request.HintCount,
                    QuestionType = request.QuestionType.ToString(),
                    Grade = grade,
                    EaseFactorBefore = srsResult.OldEaseFactor,
                    EaseFactorAfter = srsResult.NewEaseFactor,
                    IntervalBefore = srsResult.OldInterval,
                    IntervalAfter = srsResult.NewInterval,
                    NextReviewBefore = srsResult.OldNextReviewDate,
                    NextReviewAfter = srsResult.NextReviewDate,
                };

                await _uow.VocabularyReviewHistory.CreateReviewHistoryAsync(reviewHistory, ct);


                await _uow.SaveChangesAsync(ct);

                await _activityLogService.TrackVocabularyReviewedAsync(userId, vocab.Id, ct);
            }


            await _activityLogService.TrackAnswerQuestionAsync(userId, vocab.Id, isCorrect);

            return new SubmitAnswerResponseDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = vocab.Word,
                NewLevel = sessionVocab.CurrentLevel,
                IsVocabularyCompleted = sessionVocab.IsCompleted
            };
        }

        // Helper: lấy catch rate theo rarity
        private static double GetPetCatchRate(PetRarity level)
        {
            return level switch
            {
                PetRarity.Common => 1.0,
                PetRarity.Uncommon => 0.8,
                PetRarity.Rare => 0.6,
                PetRarity.Epic => 0.4,
                PetRarity.Legendary => 0.2,
                _ => 1.0
            };
        }

        // Helper: tính grade SM-2 dựa trên số lần sai, độ chính xác, thời gian phản hồi trung bình và tổng số gợi ý đã dùng
        private static int CalculateSm2Grade(
            bool isCorrect,
            int attemptCount,
            double avgResponseTime,
            int totalHints)
        {
            // Failed - grade 0 or 1
            if (!isCorrect)
            {
                return totalHints > 0 ? 1 : 0;
            }

            // A word requires 4 correct answers (levels 0-3) to be completed.
            // Extra attempts mean the user answered incorrectly.
            int wrongAttempts = Math.Max(0, attemptCount - 4);

            // Correct - calculate grade based on speed and mistakes
            // Handle invalid avgResponseTime
            if (double.IsNaN(avgResponseTime) || avgResponseTime < 0)
                avgResponseTime = 10; // Default to slow

            // Perfect recall: 0 wrong attempts, fast, no hints
            if (wrongAttempts == 0 && avgResponseTime <= 5 && totalHints == 0)
                return 5;

            // Easy recall: 0 wrong attempts, moderate speed
            if (wrongAttempts == 0 && avgResponseTime <= 10)
                return 4;

            // Good recall: 0 wrong attempts but slow/used hints, OR 1 wrong attempt
            if (wrongAttempts <= 1)
                return 3;

            // Hard recall: 2 wrong attempts
            if (wrongAttempts == 2)
                return 2;

            // Barely passed: 3+ wrong attempts
            return 1;
        }

        // Helper: kiểm tra câu trả lời đúng sai dựa trên loại câu hỏi
        private static bool CheckAnswer(
            SubmitAnswerRequestDto request,
            Vocabulary vocab)
        {
            return request.QuestionType switch
            {
                QuestionType.FillInBlank or QuestionType.Listening =>
                    string.Equals(request.Answer.Trim(), vocab.Word.Trim(), StringComparison.OrdinalIgnoreCase),

                QuestionType.MultipleChoice =>
                    request.Answer.Trim() == vocab.Word,

                QuestionType.Flashcard => true,
                _ => false
            };
        }

        // Helper: đảm bảo UserVocabularyProgress tồn tại cho user và vocab
        private async Task EnsureUserVocabularyProgressAsync(
            int userId,
            int vocabId,
            CancellationToken ct)
        {
            var progress = await _uow.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(userId, vocabId, ct);

            if (progress != null) return;

            progress = new UserVocabularyProgress
            {
                UserId = userId,
                VocabularyId = vocabId,
                EasinessFactor = 2.5,
                Interval = 1,
                Repetition = 0,
                NextReviewTime = _timeProvider.UtcNow.AddDays(1)
            };

            await _uow.UserVocabularyProgress
                .CreateUserVocabularyProgressAsync(progress, ct);
        }

        // Helper: lưu AnswerRecord cho câu trả lời
        private async Task SaveAnswerRecordAsync(
            int sessionId,
            int vocabId,
            SubmitAnswerRequestDto request,
            bool isCorrect,
            CancellationToken ct)
        {
            var attempt = await _uow.AnswerRecord
                .GetAttemptCountAsync(sessionId, vocabId, request.QuestionType, ct);

            var record = new AnswerRecord
            {
                LearningSessionId = sessionId,
                VocabularyId = vocabId,
                QuestionType = request.QuestionType,
                Answer = request.Answer,
                AttemptCount = attempt + 1,
                IsCorrect = isCorrect,
                HintCount = request.HintCount,
                ResponseTimeSeconds = request.ResponseTimeSeconds,
                CreatedAt = _timeProvider.UtcNow
            };

            await _uow.AnswerRecord.CreateAnswerRecordAsync(record, ct);
        }

    }
}
