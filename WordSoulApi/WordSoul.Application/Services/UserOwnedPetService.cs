using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class UserOwnedPetService : IUserOwnedPetService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<UserOwnedPetService> _logger;

        public UserOwnedPetService(
            IUnitOfWork uow,
            IActivityLogService activityLogService,
            ILogger<UserOwnedPetService> logger)
        {
            _uow = uow;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        // ============================================================================
        // CREATE / GRANT
        // ============================================================================

        /// <summary>
        /// Gán trực tiếp một pet cho người dùng (dùng cho admin hoặc reward chắc chắn).
        /// </summary>
        public async Task<UserOwnedPetDto?> AssignPetToUserAsync(
            AssignPetDto assignDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Assigning pet {PetId} to user {UserId}", assignDto.PetId, assignDto.UserId);

            var pet = await _uow.Pet.GetPetByIdAsync(assignDto.PetId, cancellationToken);
            var user = await _uow.User.GetUserByIdAsync(assignDto.UserId, cancellationToken);

            if (pet == null || user == null)
            {
                _logger.LogWarning("Pet or User not found when assigning PetId={PetId}, UserId={UserId}", assignDto.PetId, assignDto.UserId);
                return null;
            }



            var alreadyOwned = await _uow.UserOwnedPet.CheckPetOwnedByUserAsync(assignDto.UserId, assignDto.PetId, cancellationToken);
            if (alreadyOwned)
                throw new ArgumentException("Pet đã được gán cho người dùng này rồi.");

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

            await _uow.UserOwnedPet.CreateUserOwnedPetAsync(userOwnedPet, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _activityLogService.CreateActivityLogAsync(
                assignDto.UserId, "AssignPet", $"Admin granted pet {pet.Name}", cancellationToken);

            return new UserOwnedPetDto
            {
                UserId = assignDto.UserId,
                PetId = assignDto.PetId,
                Level = userOwnedPet.Level,
                Experience = userOwnedPet.Experience,
                IsFavorite = userOwnedPet.IsFavorite,
                IsActive = userOwnedPet.IsActive,
                AcquiredAt = userOwnedPet.AcquiredAt
            };
        }

        /// <summary>
        /// Thử bắt/cấp pet ngẫu nhiên với tỷ lệ thành công (catchRate).
        /// Nếu đã sở hữu → tặng bonus XP. Nếu chưa → thử bắt theo tỷ lệ.
        /// </summary>
        /// <returns>(alreadyOwned, isSuccess, bonusXp)</returns>
        public async Task<(bool alreadyOwned, bool isSuccess, int bonusXp)> GrantPetAsync(
            int userId,
            int petId,
            double catchRate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to grant pet {PetId} to user {UserId} with catch rate {Rate:P1}", petId, userId, catchRate);

            bool alreadyOwned = await _uow.UserOwnedPet.CheckPetOwnedByUserAsync(userId, petId, cancellationToken);

            if (alreadyOwned)
            {
                const int bonusXp = 50;
                var user = await _uow.User.GetUserByIdAsync(userId, cancellationToken);
                if (user != null)
                {
                    user.XP += bonusXp;
                    await _uow.User.UpdateUserAsync(user, cancellationToken);
                    await _uow.SaveChangesAsync(cancellationToken);
                }

                await _activityLogService.CreateActivityLogAsync(userId, "PetDuplicate", $"Received {bonusXp} XP (already owned pet)", cancellationToken);
                return (true, false, bonusXp);
            }

            // Chưa sở hữu → thử bắt
            var random = new Random(Guid.NewGuid().GetHashCode());
            bool success = random.NextDouble() <= catchRate;

            if (!success)
            {
                _logger.LogInformation("Pet catch failed for user {UserId}", userId);
                return (false, false, 0);
            }

            // Thành công → tạo pet mới
            bool shouldBeActive = await _uow.UserOwnedPet.GetActivePetByUserIdAsync(userId, cancellationToken) == null;

            var newOwnedPet = new UserOwnedPet
            {
                UserId = userId,
                PetId = petId,
                Level = 1,
                Experience = 0,
                IsActive = shouldBeActive,
                IsFavorite = false,
                AcquiredAt = DateTime.UtcNow
            };

            await _uow.UserOwnedPet.CreateUserOwnedPetAsync(newOwnedPet, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var pet = await _uow.Pet.GetPetByIdAsync(petId, cancellationToken);
            await _activityLogService.CreateActivityLogAsync(
                userId, "PetCaught", $"Caught pet {pet?.Name ?? petId.ToString()}", cancellationToken);

            return (false, true, 0);
        }

        /// <summary>
        /// Tăng kinh nghiệm cho pet của người dùng, xử lý lên cấp và tiến hóa.
        /// </summary>
        public async Task<UpgradePetDto?> UpgradePetForUserAsync(
            int userId,
            int petId,
            int experience = 10,
            CancellationToken cancellationToken = default)
        {
            var ownedPet = await _uow.UserOwnedPet.GetUserOwnedPetByUserAndPetIdAsync(userId, petId, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} does not own pet {petId}");

            ownedPet.Experience += experience;

            bool isLevelUp = false;
            bool isEvolve = false;
            int? evolvedToPetId = null;

            while (ownedPet.Experience >= 100)
            {
                ownedPet.Level++;
                ownedPet.Experience -= 100;
                isLevelUp = true;

                // Lấy thông tin pet hiện tại để kiểm tra tiến hóa
                var currentPet = await _uow.Pet.GetPetByIdAsync(ownedPet.PetId, cancellationToken);
                if (currentPet?.NextEvolutionId == null ||
                    currentPet.RequiredLevel == null ||
                    ownedPet.Level < currentPet.RequiredLevel)
                    continue;

                // Có thể tiến hóa
                var evolvedPet = await _uow.Pet.GetPetByIdAsync(currentPet.NextEvolutionId.Value, cancellationToken);
                if (evolvedPet == null) continue;

                ownedPet.PetId = evolvedPet.Id;
                evolvedToPetId = evolvedPet.Id;
                isEvolve = true;

                await _activityLogService.CreateActivityLogAsync(
                    userId, "PetEvolved", $"Pet evolved from {currentPet.Name} → {evolvedPet.Name}", cancellationToken);

                break; // Chỉ tiến hóa 1 lần mỗi lần gọi
            }

            await _uow.UserOwnedPet.UpdateUserOwnedPetAsync(ownedPet, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new UpgradePetDto
            {
                PetId = ownedPet.PetId,
                Level = ownedPet.Level,
                Experience = ownedPet.Experience,
                IsLevelUp = isLevelUp,
                IsEvolved = isEvolve,
                //EvolvedToPetId = evolvedToPetId
            };
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Đặt một pet làm pet đang active (chỉ có 1 pet active tại một thời điểm).
        /// </summary>
        public async Task<UserPetDetailDto?> ActivePetAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default)
        {
            var targetPet = await _uow.UserOwnedPet.GetUserOwnedPetByUserAndPetIdAsync(userId, petId, cancellationToken)
                ?? throw new InvalidOperationException("Người dùng không sở hữu pet này.");

            // Tắt tất cả pet khác
            var allPets = await _uow.UserOwnedPet.GetAllUserOwnedPetByUserIdAsync(userId, cancellationToken);
            foreach (var pet in allPets)
            {
                if (pet.IsActive)
                {
                    pet.IsActive = false;
                    await _uow.UserOwnedPet.UpdateUserOwnedPetAsync(pet, cancellationToken);
                }
            }

            // Bật pet được chọn
            targetPet.IsActive = true;
            await _uow.UserOwnedPet.UpdateUserOwnedPetAsync(targetPet, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var petEntity = await _uow.Pet.GetPetByIdAsync(targetPet.PetId, cancellationToken);

            return new UserPetDetailDto
            {
                Id = targetPet.PetId,
                Name = petEntity?.Name ?? "Unknown",
                Description = petEntity?.Description,
                ImageUrl = petEntity?.ImageUrl,
                Level = targetPet.Level,
                Experience = targetPet.Experience,
                IsFavorite = targetPet.IsFavorite,
                IsActive = true,
                AcquiredAt = targetPet.AcquiredAt
            };
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Gỡ pet khỏi người dùng (dùng cho admin hoặc hệ thống).
        /// </summary>
        public async Task<bool> RemovePetFromUserAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default)
        {
            var ownedPet = await _uow.UserOwnedPet.GetUserOwnedPetByUserAndPetIdAsync(userId, petId, cancellationToken);
            if (ownedPet == null)
                return false;

            await _uow.UserOwnedPet.DeleteUserOwnedPetAsync(ownedPet, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Pet {PetId} removed from user {UserId}", petId, userId);
            return true;
        }
    }
}