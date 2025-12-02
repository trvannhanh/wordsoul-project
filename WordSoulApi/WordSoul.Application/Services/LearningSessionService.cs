using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.LearningSession;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.DTOs.QuizQuestion;
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
        private readonly IActivityLogService _activityLogService;
        private readonly ISetRewardPetService _setRewardPetService;

        /// <summary>
        /// Khởi tạo LearningSessionService.
        /// </summary>
        public LearningSessionService(
            IUnitOfWork uow,
            ILogger<LearningSessionService> logger,
            IUserOwnedPetService userOwnedPetService,
            IActivityLogService activityLogService,
            ISetRewardPetService setRewardPetService)
        {
            _uow = uow;
            _logger = logger;
            _userOwnedPetService = userOwnedPetService;
            _activityLogService = activityLogService;
            _setRewardPetService = setRewardPetService;
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
                    CurrentCorrectAnswered = correctAnswerNumber
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
            int userId,
            int wordCount = 5,
            CancellationToken ct = default)
        {
            if (userId <= 0) throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (wordCount <= 0) throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var existingSession = await _uow.LearningSession
                .GetExistingReviewSessionUnCompletedForUserAsync(userId, ct);

            if (existingSession != null)
            {
                _logger.LogInformation("User {UserId} already has an existing uncompleted review session", userId);

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

            var vocabularies = await _uow.SetVocabulary.GetUnreviewdVocabulariesFromSetAsync(userId, wordCount, ct);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unreviewed vocabularies for user {UserId}", userId);
                throw new InvalidOperationException("No unreviewed vocabularies available");
            }

            return await CreateSessionAsync(userId, null, SessionType.Review, vocabularies, null, null, ct);
        }

        /// <summary>
        /// Helper: tạo session và persist qua UnitOfWork.
        /// </summary>
        private async Task<LearningSessionDto> CreateSessionAsync(
            int userId,
            int? setId,
            SessionType type,
            IEnumerable<Vocabulary> vocabularies,
            int? petId,
            double? catchRate,
            CancellationToken ct)
        {
            var session = new LearningSession
            {
                UserId = userId,
                VocabularySetId = setId,
                Type = type,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                SessionVocabularies = vocabularies.Select((v, index) => new SessionVocabulary
                {
                    VocabularyId = v.Id,
                    Order = index + 1,
                    CurrentLevel = 0,
                    IsCompleted = false
                }).ToList(),
                PetId = petId,
                CatchRate = catchRate
            };

            var savedSession = await _uow.LearningSession.CreateLearningSessionAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                PetId = savedSession.PetId,
                CatchRate = catchRate,
                CurrentCorrectAnswered = 0
            };
        }

        //------------------------------------READ-------------------------------------------

        /// <summary>
        /// Lấy danh sách câu hỏi quiz cho một phiên học cụ thể.
        /// Nếu session đã complete thì trả về rỗng.
        /// </summary>
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _uow.LearningSession.GetLearningSessionByIdAsync(sessionId, ct);
            if (session?.IsCompleted == true) return Enumerable.Empty<QuizQuestionDto>();

            var sessionVocabs = await _uow.SessionVocabulary.GetSessionVocabulariesBySessionIdAsync(sessionId, ct);
            var incompleteVocabs = sessionVocabs.Where(sv => !sv.IsCompleted).ToList();
            if (!incompleteVocabs.Any())
            {
                _logger.LogInformation("All vocabularies completed for session {SessionId}", sessionId);
                return Enumerable.Empty<QuizQuestionDto>();
            }

            var allWords = sessionVocabs.Select(sv => sv.Vocabulary!.Word).ToList();

            var questions = new List<QuizQuestionDto>();
            var levelToType = new Dictionary<int, QuestionType>
            {
                {0, QuestionType.Flashcard},
                {1, QuestionType.FillInBlank},
                {2, QuestionType.MultipleChoice},
                {3, QuestionType.Listening}
            };

            foreach (var sv in incompleteVocabs)
            {
                var vocab = sv.Vocabulary;
                if (vocab == null) continue;

                var questionType = levelToType[sv.CurrentLevel];
                questions.Add(CreateQuizQuestionDto(vocab, questionType, allWords, sv.CurrentLevel > 0));
            }

            return questions.OrderBy(q => sessionVocabs.First(sv => sv.VocabularyId == q.VocabularyId).Order);
        }

        private QuizQuestionDto CreateQuizQuestionDto(Vocabulary vocab, QuestionType type, List<string> allWords, bool isRetry)
        {
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
                Options = type == QuestionType.MultipleChoice
                    ? allWords.Where(w => w != vocab.Word).OrderBy(x => Guid.NewGuid()).Take(3).Append(vocab.Word).OrderBy(x => Guid.NewGuid()).ToList()
                    : null,
                IsRetry = isRetry
            };
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
            session.EndTime = DateTime.UtcNow;
            await _uow.LearningSession.UpdateLearningSessionAsync(session, ct);

            await _activityLogService.CreateActivityLogAsync(userId, "SessionCompletion", "User completed session", ct);

            int xpEarned = sessionType == SessionType.Learning ? 10 : 5;
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
                petUpgraded = await _userOwnedPetService.UpgradePetForUserAsync(userId, activePet.Id, 10, ct);
            }

            // Commit all changes done so far
            await _uow.SaveChangesAsync(ct);

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
            if (request == null || request.VocabularyId <= 0 || string.IsNullOrWhiteSpace(request.Answer))
                throw new ArgumentException("Invalid request data");

            var userSessionExist = await _uow.LearningSession.GetExistingLearningSessionForUserAsync(userId, sessionId, ct);
            if (userSessionExist == null) throw new UnauthorizedAccessException("User does not have access to this session");

            var sessionVocab = await _uow.SessionVocabulary.GetSessionVocabularyAsync(sessionId, request.VocabularyId, ct);
            if (sessionVocab == null) throw new KeyNotFoundException("Vocabulary not found in session");

            var vocab = sessionVocab.Vocabulary;
            if (vocab == null) throw new KeyNotFoundException("Vocabulary not found");

            var levelToType = new Dictionary<int, QuestionType>
            {
                {0, QuestionType.Flashcard},
                {1, QuestionType.FillInBlank},
                {2, QuestionType.MultipleChoice},
                {3, QuestionType.Listening}
            };
            var currentType = levelToType[sessionVocab.CurrentLevel];
            if (request.QuestionType != currentType) throw new ArgumentException("Question type does not match current level");

            var attemptCount = await _uow.AnswerRecord.GetAttemptCountAsync(sessionId, request.VocabularyId, request.QuestionType, ct);

            bool isCorrect = request.QuestionType switch
            {
                QuestionType.FillInBlank or QuestionType.Listening =>
                    string.Equals(request.Answer.Trim(), vocab.Word.Trim(), StringComparison.OrdinalIgnoreCase),

                QuestionType.MultipleChoice =>
                    request.Answer.Trim() == vocab.Word,

                QuestionType.Flashcard =>
                    true,

                _ => false
            };

            var existingAnswerRecord = await _uow.AnswerRecord.GetAnswerRecordFromSession(sessionId, vocab.Id, request.QuestionType, ct);
            if (existingAnswerRecord == null)
            {
                var record = new AnswerRecord
                {
                    LearningSessionId = sessionId,
                    VocabularyId = vocab.Id,
                    QuestionType = request.QuestionType,
                    Answer = request.Answer,
                    AttemptCount = attemptCount + 1,
                    IsCorrect = isCorrect,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.AnswerRecord.CreateAnswerRecordAsync(record, ct);
            }
            else
            {
                existingAnswerRecord.Answer = request.Answer;
                existingAnswerRecord.AttemptCount = attemptCount + 1;
                existingAnswerRecord.IsCorrect = isCorrect;
                existingAnswerRecord.CreatedAt = DateTime.UtcNow;
                await _uow.AnswerRecord.UpdateAnswerRecordAsync(existingAnswerRecord, ct);
            }

            // Update level & potentially user progress/catch rate
            if (isCorrect)
            {
                sessionVocab.CurrentLevel = Math.Min(4, sessionVocab.CurrentLevel + 1);
                sessionVocab.IsCompleted = sessionVocab.CurrentLevel == 4;
                if (sessionVocab.IsCompleted)
                {
                    await UpdateUserVocabularyProgressAsync(userId, vocab, true, ct);
                }
            }
            else
            {
                sessionVocab.CurrentLevel = Math.Max(0, sessionVocab.CurrentLevel - 1);
                sessionVocab.IsCompleted = false;
                if (userSessionExist.CatchRate != null)
                {
                    userSessionExist.CatchRate -= 0.05;
                    await _uow.LearningSession.UpdateLearningSessionAsync(userSessionExist, ct);
                }
            }

            await _uow.SessionVocabulary.UpdateSessionVocabularyAsync(sessionVocab, ct);

            // Commit changes
            await _uow.SaveChangesAsync(ct);

            return new SubmitAnswerResponseDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = vocab.Word,
                AttemptNumber = attemptCount + 1,
                NewLevel = sessionVocab.CurrentLevel,
                IsVocabularyCompleted = sessionVocab.IsCompleted
            };
        }

        private async Task UpdateUserVocabularyProgressAsync(int userId, Vocabulary vocab, bool isCorrect, CancellationToken ct)
        {
            var progress = await _uow.UserVocabularyProgress.GetUserVocabularyProgressAsync(userId, vocab.Id, ct);
            if (progress == null)
            {
                progress = new UserVocabularyProgress
                {
                    UserId = userId,
                    VocabularyId = vocab.Id,
                    CorrectAttempt = 0,
                    TotalAttempt = 0,
                    ProficiencyLevel = 0
                };
                await _uow.UserVocabularyProgress.CreateUserVocabularyProgressAsync(progress, ct);
            }

            progress.CorrectAttempt += isCorrect ? 1 : 0;
            progress.TotalAttempt++;
            progress.ProficiencyLevel = Math.Min(5, 1 + (int)Math.Floor(Math.Log(Math.Max(1, progress.CorrectAttempt), 1.5)));
            progress.LastUpdated = DateTime.UtcNow;

            int daysToAdd = progress.ProficiencyLevel switch
            {
                1 => 1,
                2 => 2,
                3 => 4,
                4 => 16,
                _ => 256
            };
            progress.NextReviewTime = DateTime.UtcNow.AddDays(daysToAdd);

            await _uow.UserVocabularyProgress.UpdateUserVocabularyProgressAsync(progress, ct);
        }

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
    }
}
