using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class SetRewardPetService : ISetRewardPetService
    {
        private ISetRewardPetRepository _setRewardPetRepository;

        public SetRewardPetService(ISetRewardPetRepository setRewardPetRepository)
        {
            _setRewardPetRepository = setRewardPetRepository;
        }
        public async Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId, int milestone)
        {
            // Lấy danh sách pet từ database
            var setPets = await _setRewardPetRepository.GetPetsByVocabularySetIdAsync(vocabularySetId);

            if (!setPets.Any()) return null;

            // Xác định multiplier dựa trên milestone
            double rarityMultiplier = GetRarityMultiplier(milestone);

            // Tính trọng số cho mỗi pet
            var weightedPets = new List<(Pet Pet, double Weight)>();
            foreach (var sp in setPets)
            {
                // Pet hiếm (DropRate thấp) được tăng trọng số, pet phổ biến giữ nguyên hoặc giảm nhẹ
                double adjustedWeight = sp.DropRate <= 0.1 ? sp.DropRate * rarityMultiplier : sp.DropRate;
                weightedPets.Add((sp.Pet, adjustedWeight));
            }

            // Chuẩn hóa trọng số để tính xác suất
            double totalWeight = weightedPets.Sum(p => p.Weight);
            if (totalWeight <= 0) return weightedPets.FirstOrDefault().Pet; // Fallback nếu tất cả trọng số = 0

            var random = new Random(Guid.NewGuid().GetHashCode());
            double roll = random.NextDouble() * totalWeight; // Roll trong khoảng [0, totalWeight)

            // Chọn pet dựa trên trọng số
            double cumulativeWeight = 0;
            foreach (var (pet, weight) in weightedPets)
            {
                cumulativeWeight += weight;
                if (roll <= cumulativeWeight)
                {
                    return pet;
                }
            }

            // Fallback: trả về pet cuối cùng nếu không chọn được (đề phòng lỗi số học)
            return weightedPets.LastOrDefault().Pet;
        }

        // Hàm helper để lấy multiplier dựa trên milestone
        private static double GetRarityMultiplier(int milestone)
        {
            return milestone switch
            {
                < 5 => 1.0,    // Very Low
                < 10 => 5.0,   // Low
                < 20 => 10.0,  // Medium
                < 50 => 20.0,  // High
                _ => 50.0      // Very High
            };
        }
    }
}
