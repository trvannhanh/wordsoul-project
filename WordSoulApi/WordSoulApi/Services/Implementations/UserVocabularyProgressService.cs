using WordSoulApi.Models.DTOs.UserVocabularyProgress;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserVocabularyProgressService : IUserVocabularyProgressService
    {
        private readonly IUserVocabularyProgressRepository _userVocabularyProgressRepository;
        private readonly ILearningSessionRepository _learningSessionRepository;
        private readonly IAnswerRecordRepository _answerRecordRepository;
        public UserVocabularyProgressService(IUserVocabularyProgressRepository userVocabularyProgressRepository, ILearningSessionRepository learningSessionRepository, IAnswerRecordRepository answerRecordRepository)
        {
            _userVocabularyProgressRepository = userVocabularyProgressRepository;
            _learningSessionRepository = learningSessionRepository;
            _answerRecordRepository = answerRecordRepository;
        }



        // Cập nhật tiến độ học từ vựng sau khi hoàn thành tất cả câu hỏi trong phiên học
        public async Task<UpdateProgressResponseDto> UpdateProgressAsync(int userId, int sessionId, int vocabId)
        {
            // Kiểm tra xem người dùng có phiên học hợp lệ không
            var userSessionExist = await _learningSessionRepository.CheckUserLearningSessionExist(userId, sessionId);
            if (!userSessionExist)
                throw new UnauthorizedAccessException("User does not have access to this session");

            // Kiểm tra xem tất cả câu hỏi liên quan đến từ vựng đã được trả lời đúng chưa
            bool allCorrect = await _answerRecordRepository.CheckAllQuestionsCorrectAsync(userId, sessionId, vocabId);
            if (!allCorrect)
                throw new InvalidOperationException("Not all questions for this vocabulary have been answered correctly");

            // Lấy hoặc tạo tiến trình học từ vựng cho người dùng
            var progress = await _userVocabularyProgressRepository.GetUserVocabularyProgressAsync(userId, vocabId);
            if (progress == null)
            {
                progress = new UserVocabularyProgress
                {
                    UserId = userId,
                    VocabularyId = vocabId,
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

            return new UpdateProgressResponseDto
            {
                VocabularyId = vocabId,
                ProficiencyLevel = progress.ProficiencyLevel,
            };
        }
    }
}
