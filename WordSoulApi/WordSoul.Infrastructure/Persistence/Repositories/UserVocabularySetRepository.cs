using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
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
        public async Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet, CancellationToken cancellationToken = default)
        {
            await _context.UserVocabularySets.AddAsync(userVocabularySet, cancellationToken);
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy UserVocabularySet theo userId và vocabularySetId
        public async Task<UserVocabularySet?> GetUserVocabularySetAsync(int userId, int vocabularySetId, CancellationToken cancellationToken = default)
        {
            return await _context.UserVocabularySets
                .AsNoTracking()
                .FirstOrDefaultAsync(uvs => uvs.UserId == userId && uvs.VocabularySetId == vocabularySetId, cancellationToken);
        }

        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        public async Task<bool> CheckUserHasVocabularySetAsync(int userId, int vocabId, CancellationToken cancellationToken = default)
        {
            return await _context.UserVocabularySets
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.VocabularySetId == vocabId, cancellationToken);
        }

        //-------------------------------------UPDATE-------------------------------------------

        // Cập nhật UserVocabularySet
        public async Task UpdateUserVocabularySetAsync(UserVocabularySet userVocabularySet, CancellationToken cancellationToken = default)
        {
            _context.UserVocabularySets.Update(userVocabularySet);
            await Task.CompletedTask;
        }
    }
}