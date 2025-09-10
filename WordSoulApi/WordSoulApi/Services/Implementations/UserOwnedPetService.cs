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
        private readonly IActivityLogService _activityLogService;
        public UserOwnedPetService(ILearningSessionRepository learningSessionRepository, ISetRewardPetRepository setRewardPetRepository, IUserOwnedPetRepository userOwnedPetRepository, IUserRepository userRepository, IUserVocabularySetRepository userVocabularySetRepository, IActivityLogService activityLogService)
        {
            _learningSessionRepository = learningSessionRepository;
            _setRewardPetRepository = setRewardPetRepository;
            _userOwnedPetRepository = userOwnedPetRepository;
            _userRepository = userRepository;
            _userVocabularySetRepository = userVocabularySetRepository;
            _activityLogService = activityLogService;
        }

        // Thử cấp pet khi người dùng hoàn thành milestone
        public async Task<(Pet? grantedPet, bool alreadyOwned, int bonusXp)> TryGrantPetByMilestoneAsync(int userId, int vocabularySetId)
        {
            // Kiểm tra số session đã hoàn thành
            int completedSessions = await _userVocabularySetRepository.GetCompletedLearningSessionAsync(userId, vocabularySetId);

            // Mỗi 5 session hoàn thành sẽ có cơ hội nhận pet
            if (completedSessions % 5 != 0)
                return (null, false, 0);

            // Lấy danh sách pet có thể nhận được từ bộ từ vựng
            var setPets = await _setRewardPetRepository.GetPetsByVocabularySetIdAsync(vocabularySetId);
            if (!setPets.Any()) return (null, false, 0);

            // Chọn ngẫu nhiên một pet dựa trên DropRate độc lập
            var random = new Random(Guid.NewGuid().GetHashCode());
            var eligiblePets = new List<Pet>();
            foreach (var sp in setPets)
            {
                double roll = random.NextDouble(); // 0.0 -> 1.0
                if (roll <= sp.DropRate) // Kiểm tra nếu pet được chọn dựa trên DropRate
                {
                    eligiblePets.Add(sp.Pet);
                }
            }

            if (!eligiblePets.Any()) return (null, false, 0);

            // Chọn ngẫu nhiên 1 pet từ danh sách pet được chọn
            var chosenPet = eligiblePets[random.Next(eligiblePets.Count)];

            if (chosenPet == null) return (null, false, 0);

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
                return (chosenPet, alreadyOwned, bonusXp);
            }
            else
            {
                var newUserPet = new UserOwnedPet
                {
                    UserId = userId,
                    PetId = chosenPet.Id,
                    AcquiredAt = DateTime.UtcNow
                };
                await _userOwnedPetRepository.CreateUserOwnedPetAsync(newUserPet);
                await _activityLogService.CreateActivityAsync(userId, "AssignPet", "User granted pet");
                return (chosenPet, alreadyOwned, 0);
            }
        }
    }
}
