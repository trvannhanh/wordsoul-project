using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly ISessionVocabularyRepository _sessionVocabularyRepository;
        private readonly ISetVocabularyRepository _setVocabularyRepository;
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
        private readonly ISetRewardPetService _setRewardPetService;


        public LearningSessionService(ILearningSessionRepository sessionRepo, IVocabularyRepository vocabRepo, 
                                        IUserVocabularySetRepository userVocabularySetRepository, 
                                        ILogger<LearningSessionService> logger, IUserRepository userRepository, 
                                        IUserOwnedPetService userOwnedPetService, IAnswerRecordRepository answerRecordRepository,
                                        IActivityLogService activityLogService, IUserOwnedPetRepository userOwnedPetRepository,
                                        IPetRepository petRepository, IUserVocabularyProgressRepository userVocabularyProgressRepository,
                                        ISetVocabularyRepository setVocabularyRepository, 
                                        ISessionVocabularyRepository sessionVocabularyRepository, ISetRewardPetService setRewardPetService)
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
            _setVocabularyRepository = setVocabularyRepository;
            _sessionVocabularyRepository = sessionVocabularyRepository;
            _setRewardPetService = setRewardPetService;
        }

        //------------------------------------CREATE-----------------------------------------

        // Tạo một phiên học mới cho người dùng dựa trên bộ từ vựng đã chọn
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
                    PetId = existingSession.PetId,
                    CatchRate = existingSession.CatchRate,
                };
            }

            if (wordCount <= 0)
                throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var exists = await _userVocabularySetRepository.GetUserVocabularySetAsync(userId, setId);
            if (exists == null)
            {
                _logger.LogWarning("User {UserId} doesn't own VocabularySet {SetId}", userId, setId);
                throw new InvalidOperationException("VocabularySet doesn't exist for this user");
            }

            var vocabularies = await _setVocabularyRepository.GetUnlearnedVocabulariesFromSetAsync(userId, setId, wordCount);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unlearned vocabularies for user {UserId} in set {SetId}", userId, setId);
                throw new InvalidOperationException("No unlearned vocabularies available in this set");
            }

            var randomPet = await _setRewardPetService.GetRandomPetBySetIdAsync(setId, exists.TotalCompletedSession);

            if (randomPet == null)
            {
                _logger.LogInformation("No pets in set {SetId}", setId);
                throw new InvalidOperationException("No pets available in this set");
            }

            var petCatchRate = GetPetCatchRate(randomPet.Rarity);

            return await CreateSessionAsync(userId, setId, SessionType.Learning, vocabularies, randomPet.Id, petCatchRate);
        }

        // Tạo một phiên học ôn tập mới cho người dùng
        public async Task<LearningSessionDto> CreateReviewingSessionAsync(int userId, int wordCount = 5)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            if (wordCount <= 0)
                throw new ArgumentException("WordCount must be greater than zero.", nameof(wordCount));

            var vocabularies = await _setVocabularyRepository.GetUnreviewdVocabulariesFromSetAsync(userId, wordCount);
            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unreviewed vocabularies for user {UserId}", userId);
                throw new InvalidOperationException("No unreviewed vocabularies available");
            }

            return await CreateSessionAsync(userId, null, SessionType.Review, vocabularies, null, null);
        }

        // Hàm helper để tạo phiên học
        private async Task<LearningSessionDto> CreateSessionAsync(int userId, int? setId, SessionType type, IEnumerable<Vocabulary> vocabularies, int? petId, double? catchRate)
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
                    Order = index + 1, // Giữ nguyên order
                    CurrentLevel = 0,  // Bắt đầu từ Flashcard
                    IsCompleted = false
                }).ToList(),
                PetId = petId,
                CatchRate = catchRate
            };

            var savedSession = await _sessionRepo.CreateLearningSessionAsync(session);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList(),
                PetId = savedSession.PetId,
                CatchRate = catchRate
            };
        }

        //------------------------------------READ-------------------------------------------

        // Lấy danh sách câu hỏi quiz cho một phiên học cụ thể
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId)
        {
            // Lấy session để kiểm tra completed
            var session = await _sessionRepo.GetLearningSessionByIdAsync(sessionId);
            if (session?.IsCompleted == true) return Enumerable.Empty<QuizQuestionDto>();


            // Lấy tất cả SessionVocabulary chưa hoàn thành
            var sessionVocabs = await _sessionVocabularyRepository.GetSessionVocabulariesBySessionIdAsync(sessionId);
            var incompleteVocabs = sessionVocabs.Where(sv => !sv.IsCompleted).ToList();

            if (!incompleteVocabs.Any())
            {
                _logger.LogInformation("All vocabularies completed for session {SessionId}", sessionId);
                return Enumerable.Empty<QuizQuestionDto>(); // Sẵn sàng complete session
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
                var vocab = sv.Vocabulary; // Assume eager load hoặc query riêng
                if (vocab == null) continue;

                var questionType = levelToType[sv.CurrentLevel];

                questions.Add(CreateQuizQuestionDto(vocab, questionType, allWords, sv.CurrentLevel > 0)); // isRetry nếu level > 0
            }

            // Sort theo order hoặc random để đa dạng
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

            var sessionVocabs = await _sessionVocabularyRepository.GetSessionVocabulariesBySessionIdAsync(sessionId);
            if (sessionVocabs.Any(sv => !sv.IsCompleted))
                throw new InvalidOperationException("Not all vocabularies are completed in this session");

            // Cập nhật trạng thái session
            session.IsCompleted = true;
            session.EndTime = DateTime.UtcNow;
            await _sessionRepo.UpdateLearningSessionAsync(session);

            await _activityLogService.CreateActivityLogAsync(userId, "SessionCompletion", "User completed session");

            // Xác định XP và AP dựa trên loại phiên học
            int xpEarned = sessionType == SessionType.Learning ? 10 : 5;
            int apEarned = sessionType == SessionType.Review ? 3 : 0;

            // Xử lý logic đặc thù cho học từ mới
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
                var userVocabularySet = await _userVocabularySetRepository.GetUserVocabularySetAsync(userId, session.VocabularySetId.Value);

                if (userVocabularySet != null)
                {
                    userVocabularySet.TotalCompletedSession += 1;

                    var vocabularies = await _setVocabularyRepository.GetUnlearnedVocabulariesFromSetAsync(userId, session.VocabularySetId.Value, 1);
                    if (!vocabularies.Any())
                    {
                        userVocabularySet.IsCompleted = true;
                    }

                    await _userVocabularySetRepository.UpdateUserVocabularySetAsync(userVocabularySet);
                }
            }


            if (sessionType == SessionType.Learning && session.PetId.HasValue && session.CatchRate.HasValue)
            {
                if (!session.VocabularySetId.HasValue)
                    throw new InvalidOperationException("VocabularySetId is required for learning session");

                // Grant Pet ngay lập tức với catch rate 100% (nếu chưa sở hữu)
                var (alreadyOwned, isSuccess, bonusXp) = await _userOwnedPetService.GrantPetAsync(userId, session.PetId.Value, session.CatchRate.Value);
                isPetRewardGranted = isSuccess; // Chỉ true nếu grant mới
                isPetAlreadyOwned = alreadyOwned;
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
                    IsPetAlreadyOwned = isPetAlreadyOwned,
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

        
        // Xử lý khi người dùng gửi câu trả lời cho một câu hỏi quiz trong một phiên học cụ thể
        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request)
        {
            if (request == null || request.VocabularyId <= 0 || string.IsNullOrWhiteSpace(request.Answer))
                throw new ArgumentException("Invalid request data");

            // Kiểm tra user có tham gia session này không
            var userSessionExist = await _sessionRepo.GetExistingLearningSessionForUserAsync(userId, sessionId);
            if (userSessionExist == null)
                throw new UnauthorizedAccessException("User does not have access to this session");

            var sessionVocab = await _sessionVocabularyRepository.GetSessionVocabularyAsync(sessionId, request.VocabularyId);
            if (sessionVocab == null)
                throw new KeyNotFoundException("Vocabulary not found in session");

            var vocab = sessionVocab.Vocabulary!;
            if (vocab == null)
                throw new KeyNotFoundException("Vocabulary not found");

            var levelToType = new Dictionary<int, QuestionType>
            {
                {0, QuestionType.Flashcard},
                {1, QuestionType.FillInBlank},
                {2, QuestionType.MultipleChoice},
                {3, QuestionType.Listening}
            };
            var currentType = levelToType[sessionVocab.CurrentLevel];

            // Kiểm tra QuestionType match CurrentLevel
            if (request.QuestionType != currentType)
                throw new ArgumentException("Question type does not match current level");

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

            var existingAnswerRecord = await _answerRecordRepository.GetAnswerRecordFromSession(sessionId, vocab.Id, request.QuestionType);
            if (existingAnswerRecord == null)
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

            // Cập nhật level
            if (isCorrect)
            {
                sessionVocab.CurrentLevel = Math.Min(4, sessionVocab.CurrentLevel + 1);
                sessionVocab.IsCompleted = sessionVocab.CurrentLevel == 4;
                if (sessionVocab.IsCompleted)
                {
                    await UpdateUserVocabularyProgressAsync(userId, vocab, isCorrect);
                }
                
            }
            else
            {
                sessionVocab.CurrentLevel = Math.Max(0, sessionVocab.CurrentLevel - 1);
                sessionVocab.IsCompleted = false;
                if(userSessionExist.CatchRate != null)
                {
                    userSessionExist.CatchRate -= 0.05;
                    await _sessionRepo.UpdateLearningSessionAsync(userSessionExist);
                }    
                
            }

            await _sessionVocabularyRepository.UpdateSessionVocabularyAsync(sessionVocab);

            return new SubmitAnswerResponseDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = vocab.Word,
                AttemptNumber = attemptCount + 1,
                NewLevel = sessionVocab.CurrentLevel,
                IsVocabularyCompleted = sessionVocab.IsCompleted
            };
        }

        private async Task UpdateUserVocabularyProgressAsync(int userId, Vocabulary vocab, bool isCorrect)
        {
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

            progress.CorrectAttempt += isCorrect ? 1 : 0;
            progress.TotalAttempt++;
            progress.ProficiencyLevel = Math.Min(5, 1 + (int)Math.Floor(Math.Log(Math.Max(1, progress.CorrectAttempt), 1.5)));
            progress.LastUpdated = DateTime.UtcNow;

            // Spaced repetition
            int daysToAdd = progress.ProficiencyLevel switch
            {
                1 => 1,
                2 => 2,
                3 => 4,
                4 => 16,
                _ => 256
            };
            progress.NextReviewTime = DateTime.UtcNow.AddDays(daysToAdd);

            await _userVocabularyProgressRepository.UpdateUserVocabularyProgressAsync(progress);
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
