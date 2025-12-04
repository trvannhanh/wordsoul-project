using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class PetService : IPetService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PetService> _logger;
        private readonly IUploadAssetsService _uploadAssetsService;
        private readonly IActivityLogService _activityLogService;

        public PetService(
            IUnitOfWork uow,
            ILogger<PetService> logger,
            IUploadAssetsService uploadAssetsService,
            IActivityLogService activityLogService)
        {
            _uow = uow;
            _logger = logger;
            _uploadAssetsService = uploadAssetsService;
            _activityLogService = activityLogService;
        }

        // ============================================================================
        // CREATE
        // ============================================================================

        public async Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl)
        {
            _logger.LogInformation("Creating pet: {Name}", petDto.Name);

            if (string.IsNullOrWhiteSpace(petDto.Name))
                throw new ArgumentException("Name is required.", nameof(petDto.Name));

            var pet = new Pet
            {
                Name = petDto.Name,
                Description = petDto.Description,
                ImageUrl = imageUrl,
                Rarity = petDto.Rarity,
                Type = petDto.Type
            };

            await _uow.Pet.CreatePetAsync(pet);
            await _uow.SaveChangesAsync();

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

        public async Task<List<PetDto>> CreatePetsBulkAsync(BulkCreatePetDto bulkDto)
        {
            var createdPets = new List<PetDto>();

            foreach (var dto in bulkDto.Pets)
            {
                string? imageUrl = null;
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    (imageUrl, _) = await _uploadAssetsService.UploadImageAsync(dto.ImageFile, "pets");
                }

                var pet = new Pet
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = imageUrl,
                    Rarity = dto.Rarity,
                    Type = dto.Type,
                    BaseFormId = dto.BaseFormId,
                    NextEvolutionId = dto.NextEvolutionId,
                    RequiredLevel = dto.RequiredLevel,
                    IsActive = true
                };

                await _uow.Pet.CreatePetAsync(pet);

                createdPets.Add(new AdminPetDto
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Description = pet.Description,
                    ImageUrl = pet.ImageUrl,
                    Rarity = pet.Rarity.ToString(),
                    Type = pet.Type.ToString(),
                    BaseFormId = pet.BaseFormId,
                    NextEvolutionId = pet.NextEvolutionId,
                    RequiredLevel = pet.RequiredLevel,
                    CreatedAt = pet.CreatedAt,
                    IsActive = pet.IsActive
                });
            }

            await _uow.SaveChangesAsync();
            return createdPets;
        }

        // ============================================================================
        // READ
        // ============================================================================

        public async Task<IEnumerable<UserPetDto>> GetAllPetsAsync(
            int userId,
            string? name = null,
            PetRarity? rarity = null,
            PetType? type = null,
            bool? isOwned = null,
            int? vocabularySetId = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var pets = await _uow.Pet.GetAllPetsAsync(
                userId,
                name,
                rarity,
                type,
                isOwned,
                vocabularySetId,
                pageNumber,
                pageSize,
                cancellationToken);

            var result = pets.Select(x => new UserPetDto
            {
                Id = x.Pet.Id,
                Name = x.Pet.Name,
                Description = x.Pet.Description ?? "",
                ImageUrl = x.Pet.ImageUrl ?? "",
                Rarity = x.Pet.Rarity.ToString(),
                Type = x.Pet.Type.ToString(),
                BaseFormId = x.Pet.BaseFormId,
                NextEvolutionId = x.Pet.NextEvolutionId,
                RequiredLevel = x.Pet.RequiredLevel,
                Order = x.Pet.Order,
                IsOwned = x.IsOwned
            });

            return result;
        }

        public async Task<UserPetDetailDto?> GetPetDetailAsync(int userId, int petId)
        {
            var pet = await _uow.Pet.GetPetByIdAsync(petId);
            if (pet == null) return null;

            var owned = await _uow.UserOwnedPet.GetUserOwnedPetByUserAndPetIdAsync(userId, petId);

            return new UserPetDetailDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Description = pet.Description ?? "No description available",
                ImageUrl = pet.ImageUrl ?? "",
                Rarity = pet.Rarity.ToString(),
                Type = pet.Type.ToString(),
                Level = owned?.Level,
                Experience = owned?.Experience,
                IsFavorite = owned?.IsFavorite,
                AcquiredAt = owned?.AcquiredAt
            };
        }

        public async Task<PetDto?> GetPetByIdAsync(int id)
        {
            var pet = await _uow.Pet.GetPetByIdAsync(id);
            if (pet == null) return null;

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

        // ============================================================================
        // UPDATE
        // ============================================================================

        public async Task<AdminPetDto> UpdatePetAsync(int id, UpdatePetDto dto, string? imageUrl)
        {
            var pet = await _uow.Pet.GetPetByIdAsync(id);
            if (pet == null)
                throw new KeyNotFoundException($"Pet with ID {id} not found.");

            pet.Name = dto.Name;
            pet.Description = dto.Description;
            pet.ImageUrl = imageUrl ?? pet.ImageUrl;
            pet.Rarity = dto.Rarity;
            pet.Type = dto.Type;
            pet.IsActive = dto.IsActive;
            pet.CreatedAt = dto.CreatedAt;

            await _uow.Pet.UpdatePetAsync(pet);
            await _uow.SaveChangesAsync();

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

        public async Task<List<PetDto>> UpdatePetsBulkAsync(List<UpdatePetDto> pets)
        {
            var updatedList = new List<PetDto>();

            foreach (var dto in pets)
            {
                var pet = await _uow.Pet.GetPetByIdAsync(dto.Id);
                if (pet == null) continue;

                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    (var url, _) = await _uploadAssetsService.UploadImageAsync(dto.ImageFile, "pets");
                    pet.ImageUrl = url;
                }

                pet.Name = dto.Name;
                pet.Description = dto.Description;
                pet.Rarity = dto.Rarity;
                pet.Type = dto.Type;
                pet.BaseFormId = dto.BaseFormId;
                pet.NextEvolutionId = dto.NextEvolutionId;
                pet.RequiredLevel = dto.RequiredLevel;
                pet.IsActive = dto.IsActive;

                await _uow.Pet.UpdatePetAsync(pet);

                updatedList.Add(new AdminPetDto
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Description = pet.Description,
                    ImageUrl = pet.ImageUrl,
                    Rarity = pet.Rarity.ToString(),
                    Type = pet.Type.ToString(),
                    BaseFormId = pet.BaseFormId,
                    NextEvolutionId = pet.NextEvolutionId,
                    RequiredLevel = pet.RequiredLevel,
                    CreatedAt = pet.CreatedAt,
                    IsActive = pet.IsActive
                });
            }

            await _uow.SaveChangesAsync();
            return updatedList;
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        public async Task<bool> DeletePetAsync(int id)
        {
            await _uow.Pet.DeletePetAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePetsBulkAsync(List<int> petIds)
        {
            foreach (var id in petIds)
            {
                await _uow.Pet.DeletePetAsync(id);
            }

            await _uow.SaveChangesAsync();
            return true;
        }


    }
}
