using System;
using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;
namespace WordSoulApi.Services.Implementations
{
    public class LearningSessionService : ILearningSessionService
    {
        private readonly ILearningSessionRepository _sessionRepo;
        private readonly IVocabularyRepository _vocabRepo;
        private readonly IUserVocabularySetRepository _userVocabularySetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserOwnedPetService _userOwnedPetService;
        private readonly IAnswerRecordRepository _answerRecordRepository;
        private readonly ILogger<LearningSessionService> _logger;


        public LearningSessionService(ILearningSessionRepository sessionRepo, IVocabularyRepository vocabRepo, IUserVocabularySetRepository userVocabularySetRepository, ILogger<LearningSessionService> logger, IUserRepository userRepository, IUserOwnedPetService userOwnedPetService, IAnswerRecordRepository answerRecordRepository)
        {
            _sessionRepo = sessionRepo;
            _vocabRepo = vocabRepo;
            _userVocabularySetRepository = userVocabularySetRepository;
            _logger = logger;
            _userRepository = userRepository;
            _userOwnedPetService = userOwnedPetService;
            _answerRecordRepository = answerRecordRepository;
        }

        public async Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (setId <= 0)
                throw new ArgumentException("VocabularySetId must be greater than zero.", nameof(setId));
            if (wordCount <= 0)
                throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var exists = await _userVocabularySetRepository.CheckUserVocabualryExist(userId, setId);
            if (!exists)
            {
                _logger.LogWarning("User {UserId} doesn't own VocabularySet {SetId}", userId, setId);
                throw new InvalidOperationException("VocabularySet doesn't exist for this user");
            }

            var vocabularies = await _vocabRepo.GetUnlearnedVocabulariesFromSetAsync(userId, setId, wordCount);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unlearned vocabularies for user {UserId} in set {SetId}", userId, setId);
                throw new InvalidOperationException("No unlearned vocabularies available in this set");
            }

            return await CreateSessionAsync(userId, setId, SessionType.Learning, vocabularies);
        }

        public async Task<LearningSessionDto> CreateReviewingSessionAsync(int userId, int wordCount = 5)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (wordCount <= 0)
                throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var vocabularies = await _vocabRepo.GetUnreviewdVocabulariesFromSetAsync(userId, wordCount);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unreviewed vocabularies for user {UserId}", userId);
                throw new InvalidOperationException("No unreviewed vocabularies available");
            }

            return await CreateSessionAsync(userId, null, SessionType.Review, vocabularies);
        }

        private async Task<LearningSessionDto> CreateSessionAsync(int userId, int? setId, SessionType type, IEnumerable<Vocabulary> vocabularies)
        {
            var session = new LearningSession
            {
                UserId = userId,
                VocabularySetId = setId,
                Type = type,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                SessionVocabularies = vocabularies.Select(v => new SessionVocabulary
                {
                    VocabularyId = v.Id
                }).ToList()
            };

            var savedSession = await _sessionRepo.CreateLearningSessionAsync(session);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList()
            };
        }


        public async Task<object> CompleteSessionAsync(int userId, int sessionId, SessionType sessionType)
        {
            // Kiểm tra đầu vào
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (sessionId <= 0)
                throw new ArgumentException("SessionId must be greater than zero.", nameof(sessionId));

            // Kiểm tra quyền sở hữu session
            var session = await _sessionRepo.GetLearningSessionByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this session");

            // Kiểm tra session chưa hoàn thành
            if (session.IsCompleted)
                throw new InvalidOperationException("Session is already completed");

            // Kiểm tra tất cả câu hỏi đã đúng
            var vocabIds = await _vocabRepo.GetVocabularyIdsBySessionIdAsync(sessionId);

            foreach (var vocabId in vocabIds)
            {
                // Kiểm tra xem tất cả câu hỏi liên quan đến từ vựng đã được trả lời đúng chưa
                var allCorrect = await _answerRecordRepository.CheckAllQuestionsCorrectAsync(sessionId, vocabId);
                if (!allCorrect)
                    throw new InvalidOperationException($"Not all questions for vocabulary {vocabId} are answered correctly");
            }

            // Cập nhật trạng thái session
            session.IsCompleted = true;
            session.EndTime = DateTime.UtcNow;
            await _sessionRepo.UpdateLearningSessionAsync(session);

            // Xác định XP và AP dựa trên loại phiên học
            int xpEarned = sessionType == SessionType.Learning ? 10 : 5; // Có thể lấy từ cấu hình
            int apEarned = sessionType == SessionType.Review ? 3 : 0;   // AP chỉ cho ôn tập

            // Xử lý logic đặc thù cho học từ mới
            bool isPetRewardGranted = false;
            int petId = 0;
            string? petName = null;
            string? petDescription = null;
            string? petImageUrl = null;
            string? petRarity = null;
            string? petType = null;

            if (sessionType == SessionType.Learning)
            {
                if (!session.VocabularySetId.HasValue)
                    throw new InvalidOperationException("VocabularySetId is required for learning session");

                // Cập nhật số session đã hoàn thành trong UserVocabularySet
                await _userVocabularySetRepository.UpdateCompletedLearningSessionAsync(userId, session.VocabularySetId.Value, 1);

                // Cấp phát pet nếu hoàn thành 5 session
                var rewards = await _userOwnedPetService.TryGrantPetByMilestoneAsync(userId, session.VocabularySetId.Value);
                isPetRewardGranted = rewards.alreadyOwned;
                if (rewards.alreadyOwned)
                {
                    petId = rewards.grantedPet.Id;
                    petName = rewards.grantedPet.Name;
                    petDescription = rewards.grantedPet.Description;
                    petImageUrl = rewards.grantedPet.ImageUrl;
                    petRarity = rewards.grantedPet.Rarity.ToString();
                    petType = rewards.grantedPet.Type.ToString();
                    xpEarned += rewards.bonusXp;
                }
            }

            // Cộng XP và AP cho người dùng
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            user.XP += xpEarned;
            user.AP += apEarned; // Cập nhật AP (0 nếu là học từ mới)
            await _userRepository.UpdateUserAsync(user);

            // Trả về DTO tương ứng
            if (sessionType == SessionType.Learning)
            {
                return new CompleteLearningSessionResponseDto
                {
                    XpEarned = xpEarned,
                    IsPetRewardGranted = isPetRewardGranted,
                    PetId = petId,
                    PetName = petName,
                    Description = petDescription,
                    ImageUrl = petImageUrl,
                    PetRarity = petRarity,
                    PetType = petType,
                    Message = isPetRewardGranted ? "Learning session completed! You earned XP and a new Pet!" : "Learning session completed! You earned XP!"
                };
            }
            else
            {
                return new CompleteReviewingSessionResponseDto
                {
                    XpEarned = xpEarned,
                    ApEarned = apEarned,
                    Message = "Reviewing session completed! You earned XP and AP!"
                };
            }
        }

        // Lấy danh sách câu hỏi quiz cho một phiên học cụ thể
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId)
        {
            // Lấy danh sách từ vựng của phiên học
            var vocabularies = await _vocabRepo.GetVocabulariesBySessionIdAsync(sessionId);

            // Nếu không có từ nào
            if (!vocabularies.Any())
                return new List<QuizQuestionDto>();

            // Láy các từ cần học 
            var allWords = vocabularies.Select(v => v.Word).ToList();
            // Tạo câu hỏi mới
            var questions = new List<QuizQuestionDto>();

            // với mỗi từ
            foreach (var vocab in vocabularies)
            {
                // Flashcard
                questions.Add(new QuizQuestionDto
                {
                    VocabularyId = vocab.Id,
                    Word = vocab.Word,
                    Meaning = vocab.Meaning,
                    Pronunciation = vocab.Pronunciation,
                    ImageUrl = vocab.ImageUrl,
                    Description = vocab.Description,
                    PartOfSpeech = vocab.PartOfSpeech.ToString(),
                    CEFRLevel = vocab.CEFRLevel.ToString(),
                    QuestionType = QuestionType.Flashcard
                });

                // FillInBlank
                questions.Add(new QuizQuestionDto
                {
                    VocabularyId = vocab.Id,
                    Word = vocab.Word,
                    Meaning = vocab.Meaning,
                    QuestionType = QuestionType.FillInBlank
                });

                // MultipleChoice
                var wrongOptions = allWords.Where(w => w != vocab.Word)
                                           .OrderBy(x => Guid.NewGuid())
                                           .Take(3)
                                           .ToList();

                var options = wrongOptions.Append(vocab.Word).OrderBy(x => Guid.NewGuid()).ToList();

                questions.Add(new QuizQuestionDto
                {
                    VocabularyId = vocab.Id,
                    Word = vocab.Word,
                    Meaning = vocab.Meaning,
                    QuestionType = QuestionType.MultipleChoice,
                    Options = options
                });

                // Listening
                questions.Add(new QuizQuestionDto
                {
                    VocabularyId = vocab.Id,
                    Word = vocab.Word,
                    PronunciationUrl = vocab.PronunciationUrl,
                    QuestionType = QuestionType.Listening
                });
            }

            return questions;
        }

        // Xử lý khi người dùng gửi câu trả lời cho một câu hỏi quiz trong một phiên học cụ thể
        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request)
        {
            if (request == null || request.VocabularyId <= 0 || string.IsNullOrWhiteSpace(request.Answer))
                throw new ArgumentException("Invalid request data");

            // Kiểm tra user có tham gia session này không
            var userSessionExist = await _sessionRepo.CheckUserLearningSessionExist(userId, sessionId);
            if (!userSessionExist)
                throw new UnauthorizedAccessException("User does not have access to this session");

            // Lấy từ vựng
            var vocab = await _vocabRepo.GetVocabularyByIdAsync(request.VocabularyId);
            if (vocab == null) throw new KeyNotFoundException("Vocabulary not found");

            // Số lần attempt trước đó
            var attemptCount = await _answerRecordRepository.GetAttemptCountAsync(sessionId, request.VocabularyId, request.QuestionType);

            // Kiểm tra đúng/sai
            bool isCorrect = request.QuestionType switch
            {
                QuestionType.FillInBlank or QuestionType.Listening =>
                    string.Equals(request.Answer.Trim(), vocab.Word.Trim(), StringComparison.OrdinalIgnoreCase),

                QuestionType.MultipleChoice =>
                    request.Answer.Trim() == vocab.Word,

                QuestionType.Flashcard =>
                    true, // Flashcard luôn đúng

                _ => false
            };

            var existingAnswerRecord = await _answerRecordRepository.GetAnswerRecordFromSession(sessionId, vocab.Id,request.QuestionType);
            if(existingAnswerRecord == null)
            {
                // Tạo bản ghi
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

                await _answerRecordRepository.CreateAnswerRecordAsync(record);
            }
            else
            {
                existingAnswerRecord.Answer = request.Answer;
                existingAnswerRecord.AttemptCount = attemptCount + 1;
                existingAnswerRecord.IsCorrect = isCorrect;
                existingAnswerRecord.CreatedAt = DateTime.UtcNow;

                await _answerRecordRepository.UpdateAnswerRecordAsync(existingAnswerRecord);
            }

            return new SubmitAnswerResponseDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = vocab.Word,
                AttemptNumber = attemptCount + 1
            };
        }

    }
}
