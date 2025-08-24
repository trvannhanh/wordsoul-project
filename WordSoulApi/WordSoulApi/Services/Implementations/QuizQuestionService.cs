using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly IQuizQuestionRepository _quizQuestionRepository;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly IAnswerRecordRepository _answerRecordRepository;
        private readonly ILearningSessionRepository _learningSessionRepository;
        public QuizQuestionService(IQuizQuestionRepository quizQuestionRepository, IVocabularyRepository vocabularyRepository, IAnswerRecordRepository answerRecordRepository, ILearningSessionRepository learningSessionRepository)
        {
            _quizQuestionRepository = quizQuestionRepository;
            _vocabularyRepository = vocabularyRepository;
            _answerRecordRepository = answerRecordRepository;
            _learningSessionRepository = learningSessionRepository;
        }

        // Lấy tất cả các câu hỏi quiz (dành cho Admin)
        public async Task<IEnumerable<AdminQuizQuestionDto>> GetAllQuizQuestionsAsync()
        {
            var quizQuestions = await _quizQuestionRepository.GetAllQuizQuestionsAsync();
            var quizQuestionDtos = new List<AdminQuizQuestionDto>();
            foreach (var quizQuestion in quizQuestions)
            {
                quizQuestionDtos.Add(new AdminQuizQuestionDto
                {
                    Id = quizQuestion.Id,
                    Prompt = quizQuestion.Prompt,
                    Options = quizQuestion.Options,
                    CorrectAnswer = quizQuestion.CorrectAnswer,
                    Explanation = quizQuestion.Explanation,
                    QuestionType = quizQuestion.QuestionType,
                    VocabularyId = quizQuestion.VocabularyId,
                    IsActive = quizQuestion.IsActive
                });

            }
            return quizQuestionDtos;

        }

        // Lấy câu hỏi quiz theo ID (trả về khác nhau giữa Admin và User thường)
        public async Task<QuizQuestionDto?> GetQuizQuestionByIdAsync(int id)
        {
            var quizQuestion = await _quizQuestionRepository.GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return null;

            //// if (User.IsInRole("Admin"))
            //{
            //    return new AdminQuizQuestionDto
            //    {
            //        Id = quizQuestion.Id,
            //        Prompt = quizQuestion.Prompt,
            //        Options = quizQuestion.Options,
            //        CorrectAnswer = quizQuestion.CorrectAnswer,
            //        Explanation = quizQuestion.Explanation,
            //        QuestionType = quizQuestion.QuestionType,
            //        VocabularyId = quizQuestion.VocabularyId,
            //        IsActive = quizQuestion.IsActive
            //    };
            //}
            return new QuizQuestionDto
            {
                Id = quizQuestion.Id,
                Prompt = quizQuestion.Prompt,
                Options = quizQuestion.Options,
                CorrectAnswer = quizQuestion.CorrectAnswer,
                Explanation = quizQuestion.Explanation,
                QuestionType = quizQuestion.QuestionType,
                VocabularyId = quizQuestion.VocabularyId,
            };
        }

        // Tạo câu hỏi quiz (dành cho Admin)
        public async Task<AdminQuizQuestionDto> CreateQuizQuestionAsync(CreateQuizQuestionDto quizQuestionDto)
        {
            var existingVocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(quizQuestionDto.VocabularyId);
            if (existingVocabulary == null)
            {
                throw new KeyNotFoundException($"Vocabulary with ID {quizQuestionDto.VocabularyId} not found.");
            }

            var quizQuestion = new QuizQuestion
            {
                Prompt = quizQuestionDto.Prompt,
                Options = quizQuestionDto.Options,
                CorrectAnswer = quizQuestionDto.CorrectAnswer,
                Explanation = quizQuestionDto.Explanation,
                QuestionType = quizQuestionDto.QuestionType,
                VocabularyId = quizQuestionDto.VocabularyId
            };
            var createdQuizQuestion = await _quizQuestionRepository.CreateQuizQuestionAsync(quizQuestion);
            return new AdminQuizQuestionDto
            {
                Id = createdQuizQuestion.Id,
                Prompt = createdQuizQuestion.Prompt,
                Options = createdQuizQuestion.Options,
                CorrectAnswer = createdQuizQuestion.CorrectAnswer,
                Explanation = createdQuizQuestion.Explanation,
                QuestionType = createdQuizQuestion.QuestionType,
                VocabularyId = createdQuizQuestion.VocabularyId,
                IsActive = createdQuizQuestion.IsActive
            };
        }

        // Cập nhật câu hỏi quiz (dành cho Admin)
        public async Task<AdminQuizQuestionDto> UpdateQuizQuestionAsync(int id, AdminQuizQuestionDto quizQuestionDto)
        {
            var existingQuizQuestion = await _quizQuestionRepository.GetQuizQuestionByIdAsync(id);
            if (existingQuizQuestion == null)
            {
                throw new KeyNotFoundException($"QuizQuestion with ID {id} not found.");
            }
            var existingVocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(quizQuestionDto.VocabularyId);
            if (existingVocabulary == null)
            {
                throw new KeyNotFoundException($"Vocabulary with ID {quizQuestionDto.VocabularyId} not found.");
            }

            existingQuizQuestion.Id = quizQuestionDto.Id;
            existingQuizQuestion.Prompt = quizQuestionDto.Prompt;
            existingQuizQuestion.Options = quizQuestionDto.Options;
            existingQuizQuestion.CorrectAnswer = quizQuestionDto.CorrectAnswer;
            existingQuizQuestion.Explanation = quizQuestionDto.Explanation;
            existingQuizQuestion.QuestionType = quizQuestionDto.QuestionType;
            existingQuizQuestion.VocabularyId = quizQuestionDto.VocabularyId;
            existingQuizQuestion.IsActive = quizQuestionDto.IsActive;

            var updatedQuizQuestion = await _quizQuestionRepository.UpdateQuizQuestionAsync(existingQuizQuestion);
            return new AdminQuizQuestionDto
            {
                Id = updatedQuizQuestion.Id,
                Prompt = updatedQuizQuestion.Prompt,
                Options = updatedQuizQuestion.Options,
                CorrectAnswer = updatedQuizQuestion.CorrectAnswer,
                Explanation = updatedQuizQuestion.Explanation,
                QuestionType = updatedQuizQuestion.QuestionType,
                VocabularyId = updatedQuizQuestion.VocabularyId,
                IsActive = updatedQuizQuestion.IsActive
            };
        }

        // Xóa câu hỏi quiz (dành cho Admin)
        public async Task<bool> DeleteQuizQuestionAsync(int id)
        {
            return await _quizQuestionRepository.DeleteQuizQuestionAsync(id);
        }

        // Xử lý khi người dùng gửi câu trả lời cho một câu hỏi quiz trong một phiên học cụ thể
        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request)
        {

            // kiểm tra request hợp lệ
            if (request == null || request.QuestionId <= 0 || string.IsNullOrWhiteSpace(request.Answer))
                throw new ArgumentException("Invalid request data");

            // Kiểm tra xem người dùng có phiên học hợp lệ không
            var userSessionExist = await _learningSessionRepository.CheckUserLearningSessionExist(userId, sessionId);
            if (!userSessionExist)
                throw new UnauthorizedAccessException("User does not have access to this session");

            // kiểm tra question có tồn tại không
            var question = await _quizQuestionRepository.GetQuizQuestionByIdAsync(request.QuestionId);
            if (question == null)
                throw new KeyNotFoundException("Question not found");

            // kiểm tra số lần attempt trước đó
            var attemptCount = await _answerRecordRepository.GetAttemptCountAsync(userId, sessionId, request.QuestionId);

            bool isCorrect = string.Equals(
                request.Answer.Trim(),
                question.CorrectAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase
            );

            var record = new AnswerRecord
            {
                UserId = userId,
                LearningSessionId = sessionId,
                QuizQuestionId = request.QuestionId,
                Answer = request.Answer,
                AttemptCount = attemptCount + 1,
                IsCorrect = isCorrect,
                CreatedAt = DateTime.UtcNow
            };

            if (await _answerRecordRepository.ExistsAsync(userId, sessionId, request.QuestionId))
            {
                await _answerRecordRepository.UpdateAnswerRecordAsync(record);
            }
            else
            {
                await _answerRecordRepository.CreateAnswerRecordAsync(record);
            }

            return new SubmitAnswerResponseDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = question.CorrectAnswer,
                Explanation = question.Explanation,
                AttemptNumber = attemptCount + 1
            };

            
        }

        // Lấy danh sách câu hỏi quiz cho một phiên học cụ thể
        public async Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId)
        {
            var vocabIds = await _vocabularyRepository.GetVocabularyIdsBySessionIdAsync(sessionId);

            if (!vocabIds.Any())
                return new List<QuizQuestionDto>(); 

            var questions = await _quizQuestionRepository.GetQuestionsByVocabularyIdsAsync(vocabIds);

            // Lấy thứ tự enum theo định nghĩa
            var questionTypeOrder = Enum.GetValues<QuestionType>().ToList();

            var activeQuestions = questions
            .Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                Prompt = q.Prompt,
                QuestionType = q.QuestionType,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                VocabularyId = q.VocabularyId
            })
            .GroupBy(q => q.QuestionType)
            .OrderBy(g => questionTypeOrder.IndexOf(g.Key)) // sắp xếp theo enum
            .SelectMany(g => g)
            .ToList();

            return activeQuestions;
        }
    }
}
