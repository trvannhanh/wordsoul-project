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
        private readonly IPetRepository _petRepository;
        private readonly IActivityLogService _activityLogService;
        public UserOwnedPetService(ILearningSessionRepository learningSessionRepository, 
                                    ISetRewardPetRepository setRewardPetRepository, 
                                    IUserOwnedPetRepository userOwnedPetRepository, 
                                    IUserRepository userRepository, IUserVocabularySetRepository userVocabularySetRepository, 
                                    IActivityLogService activityLogService, IPetRepository petRepository)
        {
            _learningSessionRepository = learningSessionRepository;
            _setRewardPetRepository = setRewardPetRepository;
            _userOwnedPetRepository = userOwnedPetRepository;

            _userRepository = userRepository;
            _userVocabularySetRepository = userVocabularySetRepository;
            _activityLogService = activityLogService;
            _petRepository = petRepository;
        }
        //-------------------------------------CREATE-----------------------------------------
        // Gán Pet
        public async Task<UserOwnedPetDto?> AssignPetToUserAsync(AssignPetDto assignDto)
        {
            var pet = await _petRepository.GetPetByIdAsync(assignDto.PetId);
            var user = await _userRepository.GetUserByIdAsync(assignDto.UserId);
            if (pet == null || user == null) return null;

            // Kiểm tra đã gán chưa
            var existing = await _userOwnedPetRepository.CheckPetOwnedByUserAsync(assignDto.UserId, assignDto.PetId);
            if (existing) throw new ArgumentException("Pet đã được gán cho user này.");

            var userOwnedPet = new UserOwnedPet
            {
                UserId = assignDto.UserId,
                PetId = assignDto.PetId,
                Level = assignDto.InitialLevel,
                Experience = assignDto.InitialExperience,
                IsFavorite = assignDto.IsFavorite,
                IsActive = assignDto.IsActive,
                AcquiredAt = DateTime.UtcNow
            };



            await _userOwnedPetRepository.CreateUserOwnedPetAsync(userOwnedPet);

            await _activityLogService.CreateActivityLogAsync(assignDto.UserId, "AssignPet", "User granted pet");

            return new UserOwnedPetDto
            {
                PetId = assignDto.PetId,
                Level = assignDto.InitialLevel,
                Experience = assignDto.InitialExperience,
                UserId = assignDto.UserId,
                IsFavorite = assignDto.IsFavorite,
                IsActive = assignDto.IsActive,
                AcquiredAt = DateTime.UtcNow
            };
        }

        // Thử cấp pet
        public async Task<(bool alreadyOwned, bool isSuccess, int bonusXp)> GrantPetAsync(int userId, int petId, double catchRate)
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
                return (alreadyOwned, false,  bonusXp);
            }
            else
            {

                var random = new Random(Guid.NewGuid().GetHashCode());
                var eligiblePets = new List<Pet>();
                double roll = random.NextDouble(); // 0.0 -> 1.0
                if (roll <= catchRate)
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
                    await _activityLogService.CreateActivityLogAsync(userId, "AssignPet", "User granted pet");

                    return (alreadyOwned, true , 0);
                }
                else
                {
                    return (alreadyOwned, false, 0);
                }
            }
        }


        public async Task<UpgradePetDto?> UpgradePetForUserAsync(int userId, int petId, int experience = 10)
        {
            var userOwnedPet = await _userOwnedPetRepository.GetUserOwnedPetByUserAndPetIdAsync(userId, petId);
            if (userOwnedPet == null) return null;

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Người dùng không tồn tại");

            if (user.AP < experience)
                throw new InvalidOperationException("Không đủ AP để nâng cấp thú cưng");

            // Thêm kinh nghiệm
            userOwnedPet.Experience += experience;

            var isLevelUp = false;
            var isEvolve = false;

            // Xử lý nhiều lần tăng cấp nếu kinh nghiệm vượt quá 100
            while (userOwnedPet.Experience >= 100)
            {
                userOwnedPet.Level++;
                userOwnedPet.Experience -= 100;
                isLevelUp = true;

                var currentPet = await _petRepository.GetPetByIdAsync(userOwnedPet.PetId); // Làm mới dữ liệu thú cưng
                if (currentPet == null) break; // Kiểm tra an toàn

                // Kiểm tra tiến hóa
                if (currentPet.RequiredLevel != null && userOwnedPet.Level >= currentPet.RequiredLevel && currentPet.NextEvolutionId.HasValue)
                {
                    var evolvedPet = await _petRepository.GetPetByIdAsync(currentPet.NextEvolutionId.Value);
                    if (evolvedPet != null)
                    {
                        userOwnedPet.PetId = evolvedPet.Id; // Cập nhật sang thú cưng tiến hóa
                        isEvolve = true;
                        await _activityLogService.CreateActivityLogAsync(userId, "PetEvolved", $"Thú cưng {petId} đã tiến hóa thành {evolvedPet.Id}");
                        break; // Dừng tăng cấp sau khi tiến hóa
                    }
                }
            }

            // Giới hạn kinh nghiệm để tránh giá trị âm
            if (userOwnedPet.Experience < 0) userOwnedPet.Experience = 0;

            var updatedUser = await _userRepository.UpdateUserXPAndAPAsync(userId, 0, -experience);
            await _userOwnedPetRepository.UpdateUserOwnedPetAsync(userOwnedPet);

            return new UpgradePetDto
            {
                PetId = userOwnedPet.PetId,
                Level = userOwnedPet.Level,
                Experience = userOwnedPet.Experience,
                IsLevelUp = isLevelUp,
                IsEvolved = isEvolve,
                AP = updatedUser.AP
            };
        }

        //----------------------------UPDATE-------------------
        public async Task<UserPetDetailDto?> ActivePetAsync(int userId, int petId)
        {
            var ownedPet = await _userOwnedPetRepository.GetUserOwnedPetByUserAndPetIdAsync(userId, petId);
            if (ownedPet == null) throw new InvalidOperationException();

            var allOwnedPets = await _userOwnedPetRepository.GetAllUserOwnedPetByUserIdAsync(userId);

            foreach (var op in allOwnedPets) {
                op.IsActive = false;
                await _userOwnedPetRepository.UpdateUserOwnedPetAsync(op);
            }

            ownedPet.IsActive = true;

            await _userOwnedPetRepository.UpdateUserOwnedPetAsync(ownedPet);
            return new UserPetDetailDto
            {
                Id = ownedPet.PetId,
                Name = ownedPet.Pet.Name,
                Description = ownedPet.Pet.Description,
                ImageUrl = ownedPet.Pet.ImageUrl,
                Level = ownedPet.Level,
                Experience = ownedPet.Experience
            };
            
        }

        //-----------------------------DELETE------------------

        // Gỡ Pet khỏi User
        public async Task<bool> RemovePetFromUserAsync(int userId, int petId)
        {
            var userOwnedPet = await _userOwnedPetRepository.GetUserOwnedPetByUserAndPetIdAsync(userId, petId);
            if (userOwnedPet == null) return false;

            await _userOwnedPetRepository.DeleteUserOwnedPetAsync(userOwnedPet);
            return true;
        }

    }
}
