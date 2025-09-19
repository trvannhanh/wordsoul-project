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

        //------------------------------- CREATE -----------------------------------
        // Thêm sự sở hữu một bộ từ vựng cho người dùng
        public async Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet)
        {
            _context.UserVocabularySets.Add(userVocabularySet);
            await _context.SaveChangesAsync();
        }

        // -------------------------------------READ-------------------------------------------

        // Lấy UserVocabularySet theo userId và vocabularySetId
        public async Task<UserVocabularySet?> GetUserVocabularySetAsync(int userId, int vocabularySetId)
        {
            return await _context.UserVocabularySets
                .AsNoTracking()
                .FirstOrDefaultAsync(uvs => uvs.UserId == userId && uvs.VocabularySetId == vocabularySetId);
        }




        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        public async Task<bool> CheckUserVocabularyExist(int userId, int vocabId)
        {
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            var exists = await _context.UserVocabularySets
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.VocabularySetId == vocabId);
            return exists;
        }


        //--------------------------------------- UPDATE ---------------------------------------

        // Cập nhật UserVocabularySet
        public async Task UpdateUserVocabularySetAsync(UserVocabularySet userVocabularySet)
        {

            _context.UserVocabularySets.Update(userVocabularySet);
            await _context.SaveChangesAsync();
            
        }

        
    }
}
