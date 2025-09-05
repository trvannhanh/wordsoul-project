using Microsoft.AspNetCore.Http.HttpResults;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        private readonly IUserOwnedPetRepository _userOwnedPetRepository;
        private readonly ILogger<VocabularySetService> _logger;

        public PetService(IPetRepository petRepository, ILogger<VocabularySetService> logger, IUserOwnedPetRepository userOwnedPetRepository)
        {
            _petRepository = petRepository;
            _userOwnedPetRepository = userOwnedPetRepository;
            _logger = logger;
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
            //if role user (Authorize)
            //return new PetDto
            //{
            //    Id = pet.Id,
            //    Name = pet.Name,
            //    Description = pet.Description,
            //    ImageUrl = pet.ImageUrl,
            //    Rarity = pet.Rarity,
            //    Type = pet.Type
            //};
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


        // Cập nhật pet
        //public async Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto)
        //{
        //    var existingPet = await _petRepository.GetPetByIdAsync(id);
        //    if (existingPet == null)
        //    {
        //        throw new KeyNotFoundException($"Pet with ID {id} not found.");
        //    }

        //    existingPet.Name = petDto.Name;
        //    existingPet.Description = petDto.Description;
        //    existingPet.ImageUrl = petDto.ImageUrl;
        //    existingPet.Rarity = petDto.Rarity;
        //    existingPet.Type = petDto.Type;
        //    existingPet.IsActive = petDto.IsActive;
        //    existingPet.CreatedAt = petDto.CreatedAt;

        //    var updatedPet = await _petRepository.UpdatePetAsync(existingPet);
        //    return new AdminPetDto
        //    {
        //        Id = updatedPet.Id,
        //        Name = updatedPet.Name,
        //        Description = updatedPet.Description,
        //        ImageUrl = updatedPet.ImageUrl,
        //        Rarity = updatedPet.Rarity,
        //        Type = updatedPet.Type,
        //        CreatedAt = updatedPet.CreatedAt,
        //        IsActive = updatedPet.IsActive
        //    };
        //}
         
        // Xóa pet theo ID
        public async Task<bool> DeletePetAsync(int id)
        {
            return await _petRepository.DeletePetAsync(id);
        }


    }
}
