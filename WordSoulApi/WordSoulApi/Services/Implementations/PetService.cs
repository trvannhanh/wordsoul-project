using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        private readonly IUserOwnedPetRepository _userOwnedPetRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<VocabularySetService> _logger;
        private readonly IUploadAssetsService _uploadAssetsService;
        private readonly IActivityLogService _activityLogService;

        public PetService(IPetRepository petRepository, ILogger<VocabularySetService> logger, IUserOwnedPetRepository userOwnedPetRepository, IUploadAssetsService uploadAssetsService, IUserRepository userRepository, IActivityLogService activityLogService)
        {
            _petRepository = petRepository;
            _userOwnedPetRepository = userOwnedPetRepository;
            _logger = logger;
            _uploadAssetsService = uploadAssetsService;
            _userRepository = userRepository;
            _activityLogService = activityLogService;
        }

        // Lây tất cả pet
        public async Task<IEnumerable<UserPetDto>> GetAllPetsAsync(
            int userId,
            string? name,
            PetRarity? rarity,
            PetType? type,
            bool? isOwned,
            int pageNumber,
            int pageSize)
        {
            var pets = await _petRepository.GetAllPetsAsync(
                userId, name, rarity, type, isOwned, pageNumber, pageSize);

            return pets.Select(p => new UserPetDto
            {
                Id = p.Pet.Id,
                Name = p.Pet.Name,
                Description = p.Pet.Description ?? "No description available",
                ImageUrl = p.Pet.ImageUrl ?? "",
                Rarity = p.Pet.Rarity.ToString(),
                Type = p.Pet.Type.ToString(),
                isOwned = p.IsOwned
            });
        }


        // Lấy pet theo ID
        public async Task<PetDto?> GetPetByIdAsync(int id)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null) return null;
            // cần thêm if role admin
            return new AdminPetDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Description = pet.Description,
                ImageUrl = pet.ImageUrl,
                Rarity = pet.Rarity.ToString(),
                Type = pet.Type.ToString(),
                CreatedAt = pet.CreatedAt,
                IsActive = pet.IsActive
            };
        }


        // Tạo pet mới
        public async Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl)
        {
            _logger.LogInformation("Creating pet with title: {Name}", petDto.Name);

            if (string.IsNullOrWhiteSpace(petDto.Name))
            {
                _logger.LogError("Title is required for creating a pet");
                throw new ArgumentException("Name is required.", nameof(petDto.Name));
            }

            var pet = new Pet
            {
                Name = petDto.Name,
                Description = petDto.Description,
                ImageUrl = imageUrl,
                Rarity = petDto.Rarity,
                Type = petDto.Type
            };

            var createdPet = await _petRepository.CreatePetAsync(pet);
            return new AdminPetDto
            {
                Id = createdPet.Id,
                Name = createdPet.Name,
                Description = createdPet.Description,
                ImageUrl = createdPet.ImageUrl,
                Rarity = createdPet.Rarity.ToString(),
                Type = createdPet.Type.ToString(),
                CreatedAt = createdPet.CreatedAt,
                IsActive = createdPet.IsActive
            };
        }


        //Cập nhật pet
        public async Task<AdminPetDto> UpdatePetAsync(int id, UpdatePetDto petDto, string? imageUrl)
        {
            var existingPet = await _petRepository.GetPetByIdAsync(id);
            if (existingPet == null)
            {
                throw new KeyNotFoundException($"Pet with ID {id} not found.");
            }

            existingPet.Name = petDto.Name;
            existingPet.Description = petDto.Description;
            existingPet.ImageUrl = imageUrl;
            existingPet.Rarity = petDto.Rarity;
            existingPet.Type = petDto.Type;
            existingPet.IsActive = petDto.IsActive;
            existingPet.CreatedAt = petDto.CreatedAt;

            var updatedPet = await _petRepository.UpdatePetAsync(existingPet);
            return new AdminPetDto
            {
                Id = updatedPet.Id,
                Name = updatedPet.Name,
                Description = updatedPet.Description,
                ImageUrl = updatedPet.ImageUrl,
                Rarity = updatedPet.Rarity.ToString(),
                Type = updatedPet.Type.ToString(),
                CreatedAt = updatedPet.CreatedAt,
                IsActive = updatedPet.IsActive
            };
        }

        // Xóa pet theo ID
        public async Task<bool> DeletePetAsync(int id)
        {
            return await _petRepository.DeletePetAsync(id);
        }

        // Bulk create pets (tạo từng pet và upload image nếu có)
        public async Task<List<PetDto>> CreatePetsBulkAsync(BulkCreatePetDto bulkDto)
        {
            var createdPets = new List<PetDto>();
            foreach (var petDto in bulkDto.Pets)
            {
                string? imageUrl = null;
                string? publicId = null;

                Console.WriteLine($"ImageFile: {petDto.ImageFile?.FileName}, Length: {petDto.ImageFile?.Length}");
                if (petDto.ImageFile != null && petDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(petDto.ImageFile, "pets");
                }

                var pet = new Pet
                {
                    Name = petDto.Name,
                    Description = petDto.Description,
                    ImageUrl = imageUrl,
                    Rarity = petDto.Rarity,
                    Type = petDto.Type,
                    BaseFormId = petDto.BaseFormId,
                    NextEvolutionId = petDto.NextEvolutionId,
                    RequiredLevel = petDto.RequiredLevel,
                    IsActive = true
                };

                var createdPet = await _petRepository.CreatePetAsync(pet);
                createdPets.Add(new AdminPetDto
                {
                    Id = createdPet.Id,
                    Name = createdPet.Name,
                    Description = createdPet.Description,
                    ImageUrl = createdPet.ImageUrl,
                    Rarity = createdPet.Rarity.ToString(),
                    Type = createdPet.Type.ToString(),
                    BaseFormId = createdPet.BaseFormId,
                    NextEvolutionId = createdPet.NextEvolutionId,
                    RequiredLevel = createdPet.RequiredLevel,
                    CreatedAt = createdPet.CreatedAt,
                    IsActive = createdPet.IsActive
                }); 
            }

            return createdPets;
        }

        // Bulk update pets 
        public async Task<List<PetDto>> UpdatePetsBulkAsync(List<UpdatePetDto> pets)
        {
            var updatedPets = new List<PetDto>();
            foreach (var petDto in pets)
            {
                var pet = await _petRepository.GetPetByIdAsync(petDto.Id);
                if (pet == null) continue;  // Skip nếu không tồn tại

                // Update fields
                pet.Name = petDto.Name;
                pet.Description = petDto.Description;
                string? imageUrl = null;
                string? publicId = null;

                if (petDto.ImageFile != null && petDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(petDto.ImageFile, "pets");
                    pet.ImageUrl = imageUrl;
                }
                pet.Rarity = petDto.Rarity;
                pet.Type = petDto.Type;
                pet.BaseFormId = petDto.BaseFormId;
                pet.NextEvolutionId = petDto.NextEvolutionId;
                pet.RequiredLevel = petDto.RequiredLevel;
                pet.IsActive = petDto.IsActive;

                var updatedPet = await _petRepository.UpdatePetAsync(pet);
                updatedPets.Add(new AdminPetDto
                {
                    Id = updatedPet.Id,
                    Name = updatedPet.Name,
                    Description = updatedPet.Description,
                    ImageUrl = updatedPet.ImageUrl,
                    Rarity = updatedPet.Rarity.ToString(),
                    Type = updatedPet.Type.ToString(),
                    BaseFormId = updatedPet.BaseFormId,
                    NextEvolutionId = updatedPet.NextEvolutionId,
                    RequiredLevel = updatedPet.RequiredLevel,
                    CreatedAt = updatedPet.CreatedAt,
                    IsActive = updatedPet.IsActive
                });
            }
            return updatedPets;
        }

        // Bulk delete pets
        public async Task<bool> DeletePetsBulkAsync(List<int> petIds)
        {
            foreach (var id in petIds)
            {
                var pet = await _petRepository.GetPetByIdAsync(id);
                if (pet != null)
                {
                    await _petRepository.DeletePetAsync(pet.Id);
                }
            }
            return true;
        }


        // Assign pet to user
        public async Task<UserOwnedPetDto?> AssignPetToUserAsync(AssignPetDto assignDto)
        {
            var pet = await _petRepository.GetPetByIdAsync(assignDto.PetId);
            var user = await _userRepository.GetUserByIdAsync(assignDto.UserId);  
            if (pet == null || user == null) return null;

            // Kiểm tra đã gán chưa
            var existing = await _userOwnedPetRepository.CheckExistsUserOwnedPetAsync(assignDto.UserId, assignDto.PetId);
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

            await _activityLogService.CreateActivityAsync(assignDto.UserId, "AssignPet", "User granted pet");

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


        // Remove pet from user
        public async Task<bool> RemovePetFromUserAsync(int userId, int petId)
        {
            var userOwnedPet = await _userOwnedPetRepository.GetUserOwnedPetByUserAndPetIdAsync(userId, petId);
            if (userOwnedPet == null) return false;

            await _userOwnedPetRepository.DeleteUserOwnedPetAsync(userOwnedPet);
            return true;
        }

        // Evolve pet for user (kiểm tra exp >= required level của next form)
        public async Task<UserOwnedPetDto?> EvolvePetForUserAsync(EvolvePetDto evolveDto)
        {
            var userOwnedPet = await _userOwnedPetRepository.GetUserOwnedPetByUserAndPetIdAsync(evolveDto.UserId, evolveDto.PetId);
            if (userOwnedPet == null) return null;

            userOwnedPet.Experience += evolveDto.ExperienceToAdd;

            var currentPet = userOwnedPet.Pet;
            if (userOwnedPet.Experience >= currentPet.RequiredLevel && currentPet.NextEvolutionId.HasValue)
            {
                // Evolve: Chuyển sang form mới
                var evolvedPet = await _petRepository.GetPetByIdAsync(currentPet.NextEvolutionId.Value);
                if (evolvedPet != null)
                {
                    userOwnedPet.PetId = evolvedPet.Id;  // Cập nhật PetId sang form mới
                    userOwnedPet.Level++;  // Tăng level
                }
            }

            await _userOwnedPetRepository.UpdateUserOwnedPetAsync(userOwnedPet);

            await _activityLogService.CreateActivityAsync(evolveDto.UserId, "PetEvolved", $"Pet {evolveDto.PetId} evolved");
            return new UserOwnedPetDto 
            {
                PetId = userOwnedPet.PetId,
                Level = userOwnedPet.Level,
                Experience = userOwnedPet.Experience,
                UserId = userOwnedPet.UserId,
                IsFavorite = userOwnedPet.IsFavorite,
                IsActive = userOwnedPet.IsActive,
                AcquiredAt = userOwnedPet.AcquiredAt,
            };
        }

    }
}
