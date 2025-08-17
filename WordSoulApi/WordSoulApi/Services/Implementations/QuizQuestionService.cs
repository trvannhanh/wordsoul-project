using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly IQuizQuestionRepository _quizQuestionRepository;
        private readonly IVocabularyRepository _vocabularyRepository;
        public QuizQuestionService(IQuizQuestionRepository quizQuestionRepository, IVocabularyRepository vocabularyRepository)
        {
            _quizQuestionRepository = quizQuestionRepository;
            _vocabularyRepository = vocabularyRepository;
        }

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

        public async Task<AdminQuizQuestionDto?> GetQuizQuestionByIdAsync(int id)
        {
            var quizQuestion = await _quizQuestionRepository.GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return null;
            return new AdminQuizQuestionDto
            {
                Id = quizQuestion.Id,
                Prompt = quizQuestion.Prompt,
                Options = quizQuestion.Options,
                CorrectAnswer = quizQuestion.CorrectAnswer,
                Explanation = quizQuestion.Explanation,
                QuestionType = quizQuestion.QuestionType,
                VocabularyId = quizQuestion.VocabularyId,
                IsActive = quizQuestion.IsActive
            };
        }

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

        public async Task<bool> DeleteQuizQuestionAsync(int id)
        {
            return await _quizQuestionRepository.DeleteQuizQuestionAsync(id);
        }
    }
}
