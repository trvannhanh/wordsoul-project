using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserVocabularySetRepository : IUserVocabularySetRepository
    {
        private readonly WordSoulDbContext _context;
        public UserVocabularySetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        public async Task<bool> CheckUserVocabualryExist(int userId, int vocabId)
        {
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            var exists = await _context.UserVocabularySets
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.VocabularySetId == vocabId);
            return exists;
        }

        // Thêm sự sở hữu một bộ từ vựng cho người dùng
        public async Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet)
        {
            _context.UserVocabularySets.Add(userVocabularySet);
            await _context.SaveChangesAsync();
        }

        // Đếm số session đã hoàn thành cho một người dùng trong một bộ từ vựng
        public async Task<int> GetCompletedLearningSessionAsync(int userId, int vocabularySetId)
        {
            // 1. Lấy số session đã hoàn thành từ bảng UserVocabularySets
            var userVocabSet = await _context.UserVocabularySets
                .AsNoTracking()
                .Where(uvs => uvs.UserId == userId && uvs.VocabularySetId == vocabularySetId)
                .Select(uvs => uvs.totalCompletedSession)
                .FirstOrDefaultAsync();

            return userVocabSet;
        }

        // Cập nhật số session đã hoàn thành cho một người dùng trong một bộ từ vựng
        public async Task UpdateCompletedLearningSessionAsync(int userId, int vocabularySetId, int increment = 1)
        {
            var userVocabSet = await _context.UserVocabularySets
                .FirstOrDefaultAsync(uvs => uvs.UserId == userId && uvs.VocabularySetId == vocabularySetId);
            if (userVocabSet != null)
            {
                userVocabSet.totalCompletedSession += increment;
                _context.UserVocabularySets.Update(userVocabSet);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("User does not own this vocabulary set.");
            }
        }
    }
}
