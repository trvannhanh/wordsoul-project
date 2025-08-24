using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserVocabularyProgressRepository : IUserVocabularyProgressRepository
    {
        private readonly WordSoulDbContext _context;
        public UserVocabularyProgressRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy tiến trình học từ vựng của người dùng theo userId và vocabularyId
        public async Task<UserVocabularyProgress?> GetUserVocabularyProgressAsync(int userId, int vocabularyId)
        {
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            return await _context.UserVocabularyProgresses
                 .AsNoTracking()
                 .FirstOrDefaultAsync(p => p.UserId == userId && p.VocabularyId == vocabularyId);
        }


        // Tạo mới tiến trình học từ vựng cho người dùng
        public async Task<UserVocabularyProgress> CreateUserVocabularyProgressAsync(UserVocabularyProgress progress)
        {
            _context.UserVocabularyProgresses.Add(progress);
            await _context.SaveChangesAsync();
            return progress;
        }


        // Cập nhật tiến trình học từ vựng cho người dùng
        public async Task<UserVocabularyProgress> UpdateUserVocabularyProgressAsync(UserVocabularyProgress progress)
        {
            _context.UserVocabularyProgresses.Update(progress);
            await _context.SaveChangesAsync();
            return progress;
        }
    }
}
