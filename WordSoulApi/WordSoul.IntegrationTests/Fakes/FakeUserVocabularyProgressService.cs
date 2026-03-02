
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeUserVocabularyProgressService : IUserVocabularyProgressService
    {
        public Task<UserProgressDto> GetUserProgressAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var dto = new UserProgressDto
            {
                ReviewWordCount = 0,
                NextReviewTime = null,
                VocabularyStats = new List<LevelStatDto>()
            };

            return Task.FromResult(dto);
        }
    }
}
