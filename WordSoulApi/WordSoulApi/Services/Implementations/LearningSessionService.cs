using Microsoft.EntityFrameworkCore;
using System;
using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
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
        private readonly IUserOwnedPetRepository _userOwnedPetRepository;
        private readonly IPetRepository _petRepository;
        private readonly ILogger<LearningSessionService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserVocabularyProgressRepository _userVocabularyProgressRepository;


        public LearningSessionService(ILearningSessionRepository sessionRepo, IVocabularyRepository vocabRepo, IUserVocabularySetRepository userVocabularySetRepository, ILogger<LearningSessionService> logger, IUserRepository userRepository, IUserOwnedPetService userOwnedPetService, IAnswerRecordRepository answerRecordRepository, IActivityLogService activityLogService, IUserOwnedPetRepository userOwnedPetRepository, IPetRepository petRepository, IUserVocabularyProgressRepository userVocabularyProgressRepository)
        {
            _sessionRepo = sessionRepo;
            _vocabRepo = vocabRepo;
            _userVocabularySetRepository = userVocabularySetRepository;
            _logger = logger;
            _userRepository = userRepository;
            _userOwnedPetService = userOwnedPetService;
            _answerRecordRepository = answerRecordRepository;
            _activityLogService = activityLogService;
            _userOwnedPetRepository = userOwnedPetRepository;
            _petRepository = petRepository;
            _userVocabularyProgressRepository = userVocabularyProgressRepository;
        }

        public async Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (setId <= 0)
                throw new ArgumentException("VocabularySetId must be greater than zero.", nameof(setId));

            var existingSession = await _sessionRepo.GetExistingLearningSessionUnCompletedForUserAsync(userId, setId);
            if (existingSession != null)
            {
                _logger.LogInformation("User {UserId} already has an existing uncompleted learning session for set {SetId}", userId, setId);
                return new LearningSessionDto
                {
                    Id = existingSession.Id,
                    IsCompleted = existingSession.IsCompleted,
                    VocabularyIds = existingSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                    PetId = existingSession.PetId 
                };
            }

            if (wordCount <= 0)
                throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var exists = await _userVocabularySetRepository.CheckUserVocabularyExist(userId, setId);
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

            var randomPet = await _userOwnedPetRepository.GetRandomPetBySetIdAsync(setId); 

            return await CreateSessionAsync(userId, setId, SessionType.Learning, vocabularies, randomPet?.Id);
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

            return await CreateSessionAsync(userId, null, SessionType.Review, vocabularies, null);
        }

        private async Task<LearningSessionDto> CreateSessionAsync(int userId, int? setId, SessionType type, IEnumerable<Vocabulary> vocabularies, int? petId)
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
                }).ToList(),
                PetId = petId
            };

            var savedSession = await _sessionRepo.CreateLearningSessionAsync(session);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                PetId = savedSession.PetId 
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
                var allCorrect = await _answerRecordRepository.CheckAllQuestionsCorrectAsync(sessionId, vocabId);
                if (!allCorrect)
                    throw new InvalidOperationException($"Not all questions for vocabulary {vocabId} are answered correctly");
            }

            // Cập nhật trạng thái session
            session.IsCompleted = true;
            session.EndTime = DateTime.UtcNow;
            await _sessionRepo.UpdateLearningSessionAsync(session);

            await _activityLogService.CreateActivityAsync(userId, "SessionCompletion", "User completed session");

            // Xác định XP và AP dựa trên loại phiên học
            int xpEarned = sessionType == SessionType.Learning ? 10 : 5;
            int apEarned = sessionType == SessionType.Review ? 3 : 0;

            // Xử lý logic đặc thù cho học từ mới
            bool isPetRewardGranted = false;
            int petId = 0;
            string? petName = null;
            string? petDescription = null;
            string? petImageUrl = null;
            string? petRarity = null;
            string? petType = null;

            if (sessionType == SessionType.Learning && session.VocabularySetId.HasValue)
            {
                var userVocabularySet = await _userVocabularySetRepository.GetUserVocabularySetAsync(userId, session.VocabularySetId.Value);

                if (userVocabularySet != null)
                {
                    userVocabularySet.totalCompletedSession += 1;

                    var vocabularies = await _vocabRepo.GetUnlearnedVocabulariesFromSetAsync(userId, session.VocabularySetId.Value, 1);
                    if (!vocabularies.Any())
                    {
                        userVocabularySet.isCompleted = true;
                    }

                    await _userVocabularySetRepository.UpdateUserVocabularySetAsync(userVocabularySet);
                }
            }


            if (sessionType == SessionType.Learning && session.PetId.HasValue)
            {
                if (!session.VocabularySetId.HasValue)
                    throw new InvalidOperationException("VocabularySetId is required for learning session");

                // Grant Pet ngay lập tức với catch rate 100% (nếu chưa sở hữu)
                var (alreadyOwned, bonusXp) = await _userOwnedPetService.GrantPetAsync(userId, session.PetId.Value);
                isPetRewardGranted = !alreadyOwned; // Chỉ true nếu grant mới
                if (alreadyOwned) xpEarned += bonusXp; // Thêm bonus XP nếu đã sở hữu

                // Lấy thông tin Pet để trả về (giả sử có DbContext)
                var pet = await _petRepository.GetPetByIdAsync(session.PetId.Value);
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

            // Cộng XP và AP cho người dùng
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            await _userRepository.UpdateUserXPAndAPAsync(userId, xpEarned, apEarned);

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
                    Message = isPetRewardGranted ? "Learning session completed! You caught a Pet!" : "Learning session completed! You earned XP!"
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
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId, bool includeRetries)
        {
            var vocabularies = await _vocabRepo.GetVocabulariesBySessionIdAsync(sessionId);
            if (!vocabularies.Any()) return new List<QuizQuestionDto>();

            var allWords = vocabularies.Select(v => v.Word).ToList();
            var questions = new List<QuizQuestionDto>();

            if (includeRetries)
            {
                // Lấy các câu hỏi sai từ AnswerRecord
                var wrongAnswers = await _answerRecordRepository.GetWrongAnswersAsync(sessionId);
                foreach (var answer in wrongAnswers)
                {
                    var vocab = vocabularies.FirstOrDefault(v => v.Id == answer.VocabularyId);
                    if (vocab == null) continue;

                    questions.Add(new QuizQuestionDto
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
                        QuestionType = answer.QuestionType,
                        Options = answer.QuestionType == QuestionType.MultipleChoice
                            ? allWords.Where(w => w != vocab.Word).OrderBy(x => Guid.NewGuid()).Take(3).Append(vocab.Word).OrderBy(x => Guid.NewGuid()).ToList()
                            : null,
                        IsRetry = true
                    });
                }
            }
            else
            {
                // Tạo câu hỏi mới cho các từ chưa hoàn thành
                foreach (var vocab in vocabularies)
                {
                    var correctTypes = await _answerRecordRepository.GetCorrectAnswerTypesAsync(sessionId, vocab.Id);
                    var remainingTypes = Enum.GetValues(typeof(QuestionType))
                        .Cast<QuestionType>()
                        .Where(t => !correctTypes.Contains(t))
                        .ToList();

                    foreach (var type in remainingTypes)
                    {
                        questions.Add(new QuizQuestionDto
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
                            IsRetry = false
                        });
                    }
                }
            }

            return questions.OrderBy(q => q.QuestionType).ThenBy(q => q.VocabularyId);
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

            // Kiểm tra xem tất cả câu hỏi liên quan đến từ vựng đã được trả lời đúng chưa
            bool allCorrect = await _answerRecordRepository.CheckAllQuestionsCorrectAsync(sessionId, vocab.Id);
            if (allCorrect)
            {
                // Lấy hoặc tạo tiến trình học từ vựng cho người dùng
                var progress = await _userVocabularyProgressRepository.GetUserVocabularyProgressAsync(userId, vocab.Id);
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

                    await _userVocabularyProgressRepository.CreateUserVocabularyProgressAsync(progress);
                }

                progress.CorrectAttempt++;
                progress.TotalAttempt++;

                //1 lần đúng → level 1
                //2–3 lần đúng → level 2
                //4–6 lần đúng → level 3
                //7–12 lần đúng → level 4
                //13–20 lần đúng → level 5
                progress.ProficiencyLevel = Math.Min(5, 1 + (int)Math.Floor(Math.Log(Math.Max(1, progress.CorrectAttempt), 1.5)));
                progress.LastUpdated = DateTime.UtcNow;

                // thuật toán spaced repetition
                int daysToAdd = progress.ProficiencyLevel switch
                {
                    1 => 1, //level 1 ôn sau 1 ngày
                    2 => 2, //level 2 ôn sau 2 ngày
                    3 => 4, //level 3 ôn sau 4 ngày
                    4 => 16, //level 4 ôn sau 16 ngày
                    _ => 256 //level 5 ôn sau 256 ngày 
                             // cải thiện thuật toán này trong tương lai
                             // cân nhắc thêm cờ isMastered để đánh dấu từ đã thuộc hẳn
                };
                progress.NextReviewTime = DateTime.UtcNow.AddDays(daysToAdd);

                await _userVocabularyProgressRepository.UpdateUserVocabularyProgressAsync(progress);
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
