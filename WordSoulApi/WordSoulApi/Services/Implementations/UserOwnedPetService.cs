using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserOwnedPetService : IUserOwnedPetService
    {
        private readonly ILearningSessionRepository _learningSessionRepository;
        private readonly ISetRewardPetRepository _setRewardPetRepository;
        private readonly IUserOwnedPetRepository _userOwnedPetRepository;
        private readonly IUserVocabularySetRepository _userVocabularySetRepository;
        private readonly IUserRepository _userRepository;
        public UserOwnedPetService(ILearningSessionRepository learningSessionRepository, ISetRewardPetRepository setRewardPetRepository, IUserOwnedPetRepository userOwnedPetRepository, IUserRepository userRepository, IUserVocabularySetRepository userVocabularySetRepository)
        {
            _learningSessionRepository = learningSessionRepository;
            _setRewardPetRepository = setRewardPetRepository;
            _userOwnedPetRepository = userOwnedPetRepository;
            _userRepository = userRepository;
            _userVocabularySetRepository = userVocabularySetRepository;
        }

        // Thử cấp pet khi người dùng hoàn thành milestone
        public async Task<(int grantedPet, bool alreadyOwned, int bonusXp)> TryGrantPetByMilestoneAsync(int userId, int vocabularySetId)
        {
            // Kiểm tra số session đã hoàn thành
            int completedSessions = await _userVocabularySetRepository.GetCompletedLearningSessionAsync(userId, vocabularySetId);

            // Mỗi 5 session hoàn thành sẽ có cơ hội nhận pet
            if (completedSessions % 5 != 0)
                return (0, false, 0);

            // Lấy danh sách pet có thể nhận được từ bộ từ vựng
            var setPets = await _setRewardPetRepository.GetPetsByVocabularySetIdAsync(vocabularySetId);
            if (!setPets.Any()) return (0, false, 0);

            // Chọn ngẫu nhiên một pet dựa trên tỉ lệ rơi
            var random = new Random(Guid.NewGuid().GetHashCode());
            double roll = random.NextDouble(); // 0.0 -> 1.0
            double cumulative = 0;
            Pet? chosenPet = null;

            // Duyệt qua danh sách pet và chọn dựa trên DropRate
            foreach (var sp in setPets)
            {
                cumulative += sp.DropRate;
                if (cumulative > 1) throw new InvalidOperationException("Total DropRate exceeds 1");
                if (roll <= cumulative)
                {
                    chosenPet = sp.Pet;
                    break;
                }
            }

            if (chosenPet == null) return (0, false, 0);

            // Kiểm tra xem người dùng đã sở hữu pet này chưa
            bool alreadyOwned = await _userOwnedPetRepository.CheckPetOwnedByUserAsync(userId, chosenPet.Id);

            // Nếu đã sở hữu, thưởng thêm XP, nếu chưa thì cấp pet mới
            if (alreadyOwned)
            {
                const int bonusXp = 50;
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.XP += bonusXp;
                    await _userRepository.UpdateUserAsync(user);
                }
                return (chosenPet.Id, alreadyOwned, bonusXp);
            }
            else
            {
                var newUserPet = new UserOwnedPet
                {
                    UserId = userId,
                    PetId = chosenPet.Id,
                    AcquiredAt = DateTime.UtcNow
                };
                await _userOwnedPetRepository.AddPetToUserAsync(newUserPet);
                return (chosenPet.Id, alreadyOwned, 0);
            }
        }
    }
}
