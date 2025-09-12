using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.Pet;
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
        public async Task<(bool alreadyOwned, int bonusXp)> GrantPetAsync(int userId, int petId)
        {
            // Kiểm tra xem người dùng đã sở hữu pet này chưa
            bool alreadyOwned = await _userOwnedPetRepository.CheckPetOwnedByUserAsync(userId, petId);

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
                return (alreadyOwned, bonusXp);
            }
            else
            {
                var newUserPet = new UserOwnedPet
                {
                    UserId = userId,
                    PetId = petId,
                    AcquiredAt = DateTime.UtcNow,
                    Experience = 0,
                    Level = 1
                };
                await _userOwnedPetRepository.CreateUserOwnedPetAsync(newUserPet);
                await _activityLogService.CreateActivityAsync(userId, "AssignPet", "User granted pet");
                return (alreadyOwned, 0);
            }
        }

        


    }
}
