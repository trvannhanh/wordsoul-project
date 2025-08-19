using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<IEnumerable<PetDto>> GetAllPetsAsync()
        {
            var pets = await _petRepository.GetAllPetsAsync();
            var petDtos = new List<PetDto>();
            foreach (var pet in pets)
            {
                // cần thêm if role admin 
                petDtos.Add(new AdminPetDto
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Description = pet.Description,
                    ImageUrl = pet.ImageUrl,
                    Rarity = pet.Rarity,
                    Type = pet.Type,
                    CreatedAt = pet.CreatedAt,
                    IsActive = pet.IsActive
                });

                //if role user  (Authorize)
                //petDtos.Add(new PetDto
                //{
                //    Id = pet.Id,
                //    Name = pet.Name,
                //    Description = pet.Description,
                //    ImageUrl = pet.ImageUrl,
                //    Rarity = pet.Rarity,
                //    Type = pet.Type
                //});
            }
            return petDtos;
        }


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
                Rarity = pet.Rarity,
                Type = pet.Type,
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

        public async Task<PetDto> CreatePetAsync(CreatePetDto petDto)
        {
            var pet = new Pet
            {
                Name = petDto.Name,
                Description = petDto.Description,
                ImageUrl = petDto.ImageUrl,
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
                Rarity = createdPet.Rarity,
                Type = createdPet.Type,
                CreatedAt = createdPet.CreatedAt,
                IsActive = createdPet.IsActive
            };
        }

        public async Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto)
        {
            var existingPet = await _petRepository.GetPetByIdAsync(id);
            if (existingPet == null)
            {
                throw new KeyNotFoundException($"Pet with ID {id} not found.");
            }

            existingPet.Name = petDto.Name;
            existingPet.Description = petDto.Description;
            existingPet.ImageUrl = petDto.ImageUrl;
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
                Rarity = updatedPet.Rarity,
                Type = updatedPet.Type,
                CreatedAt = updatedPet.CreatedAt,
                IsActive = updatedPet.IsActive
            };
        }

        public async Task<bool> DeletePetAsync(int id)
        {
            return await _petRepository.DeletePetAsync(id);
        }
    }
}
