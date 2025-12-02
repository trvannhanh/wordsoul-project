using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class SetRewardPetService : ISetRewardPetService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetRewardPetService> _logger;

        public SetRewardPetService(
            IUnitOfWork uow,
            ILogger<SetRewardPetService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Lấy ngẫu nhiên một Pet làm phần thưởng dựa trên VocabularySetId và milestone hiện tại của người dùng.
        /// Pet hiếm (DropRate thấp) sẽ có cơ hội xuất hiện cao hơn khi milestone càng lớn.
        /// </summary>
        /// <param name="vocabularySetId">ID của bộ từ vựng.</param>
        /// <param name="milestone">Cột mốc hoàn thành (số lần học thành công bộ này).</param>
        /// <param name="cancellationToken">Token để hủy thao tác bất đồng bộ.</param>
        /// <returns>Pet được chọn làm phần thưởng, hoặc null nếu không có pet nào trong bộ.</returns>
        public async Task<Pet?> GetRandomPetBySetIdAsync(
            int vocabularySetId,
            int milestone,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Rolling reward pet for VocabularySetId {SetId} with milestone {Milestone}",
                vocabularySetId, milestone);

            // Lấy danh sách các pet được cấu hình làm phần thưởng cho bộ từ vựng
            var setPets = await _uow.SetRewardPet.GetPetsByVocabularySetIdAsync(vocabularySetId, cancellationToken);

            if (!setPets.Any())
            {
                _logger.LogWarning("No reward pets configured for VocabularySetId {SetId}", vocabularySetId);
                return null;
            }

            // Tính hệ số nhân cho pet hiếm dựa trên milestone
            double rarityMultiplier = GetRarityMultiplier(milestone);

            // Tạo danh sách có trọng số: pet càng hiếm càng được boost khi milestone cao
            var weightedPets = new List<(Pet Pet, double Weight)>();
            foreach (var sp in setPets)
            {
                // Nếu DropRate ≤ 0.1 → coi là pet hiếm → tăng trọng số theo milestone
                double adjustedWeight = sp.DropRate <= 0.1
                    ? sp.DropRate * rarityMultiplier
                    : sp.DropRate;

                weightedPets.Add((sp.Pet, adjustedWeight));
            }

            // Tổng trọng số
            double totalWeight = weightedPets.Sum(x => x.Weight);
            if (totalWeight <= 0)
            {
                _logger.LogWarning("Total weight is zero or negative. Falling back to first pet.");
                return weightedPets.FirstOrDefault().Pet;
            }

            // Random có seed từ Guid để tránh trùng lặp trong cùng request (nếu gọi nhiều lần)
            var random = new Random(Guid.NewGuid().GetHashCode());
            double roll = random.NextDouble() * totalWeight;

            // Weighted random selection
            double cumulative = 0;
            foreach (var (pet, weight) in weightedPets)
            {
                cumulative += weight;
                if (roll <= cumulative)
                {
                    _logger.LogInformation(
                        "Reward pet selected: {PetName} (Rarity boost: x{Multiplier})",
                        pet.Name, rarityMultiplier);

                    return pet;
                }
            }

            // Fallback cuối cùng (rất hiếm khi xảy ra do sai số float)
            var fallbackPet = weightedPets.LastOrDefault().Pet;
            _logger.LogWarning("Fallback pet selected: {PetName}", fallbackPet?.Name);
            return fallbackPet;
        }

        /// <summary>
        /// Trả về hệ số nhân cho pet hiếm dựa trên milestone của người dùng.
        /// Milestone càng cao → pet hiếm càng dễ ra.
        /// </summary>
        /// <param name="milestone">Số lần hoàn thành bộ từ vựng.</param>
        /// <returns>Hệ số nhân áp dụng cho DropRate của pet hiếm.</returns>
        private static double GetRarityMultiplier(int milestone) => milestone switch
        {
            < 5 => 1.0,   // Không boost
            < 10 => 5.0,   // Nhẹ
            < 20 => 10.0,  // Trung bình
            < 50 => 20.0,  // Cao
            _ => 50.0   // Rất cao - gần như chắc chắn ra pet hiếm
        };
    }
}