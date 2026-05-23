using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services
{
    /// <summary>
    /// Unit tests cho SetRewardPetService.GetRandomPetBySetIdAsync
    ///
    /// Trọng số (adjustedWeight) được tính:
    ///   DropRate ≤ 0.1 (rare) → DropRate × rarityMultiplier(milestone)
    ///   DropRate  > 0.1 (common) → DropRate (không boost)
    ///
    /// Milestone → rarityMultiplier:
    ///   &lt;5  → 1×  |  &lt;10 → 5×  |  &lt;20 → 10×  |  &lt;50 → 20×  |  ≥50 → 50×
    ///
    /// Chiến lược test:
    ///   - Edge cases (empty, single pet, zero weight) → hoàn toàn deterministic
    ///   - Milestone thresholds → single-pet (luôn trả về pet đó)
    ///   - Milestone boost effect → statistical test: 1000 lần, tỉ lệ rare pet
    ///     tăng mạnh từ milestone=0 → milestone=50
    /// </summary>
    public class SetRewardPetServiceTests
    {
        // ────────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────────

        private static (SetRewardPetService service, Mock<ISetRewardPetRepository> repoMock)
            CreateService()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<ISetRewardPetRepository>();
            var loggerMock = new Mock<ILogger<SetRewardPetService>>();

            uowMock.SetupGet(x => x.SetRewardPet).Returns(repoMock.Object);

            var service = new SetRewardPetService(uowMock.Object, loggerMock.Object);
            return (service, repoMock);
        }

        private static SetRewardPet MakeSetPet(int petId, double dropRate, string? name = null) =>
            new()
            {
                VocabularySetId = 1,
                PetId = petId,
                DropRate = dropRate,
                Pet = new Pet { Id = petId, Name = name ?? $"Pet_{petId}" },
            };

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 1: Không có pet nào trong set → null
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_WhenNoPetsConfigured_ShouldReturnNull()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([]);

            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: 0);

            result.Should().BeNull();
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 2: Chỉ có một common pet → luôn trả về pet đó (deterministic)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_WithSingleCommonPet_ShouldAlwaysReturnIt()
        {
            var (service, repoMock) = CreateService();
            var pet = MakeSetPet(petId: 10, dropRate: 0.50, name: "Charmander");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([pet]);

            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: 0);

            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.Name.Should().Be("Charmander");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 3: Chỉ có một rare pet (DropRate ≤ 0.1) → luôn trả về pet đó
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_WithSingleRarePet_ShouldAlwaysReturnIt()
        {
            var (service, repoMock) = CreateService();
            var rarePet = MakeSetPet(petId: 150, dropRate: 0.01, name: "Mewtwo");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([rarePet]);

            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: 3);

            result.Should().NotBeNull();
            result!.Id.Should().Be(150);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 4: Nhiều pet nhưng chỉ 1 pet có trọng số > 0 → deterministic
        // Pet với DropRate=0 luôn có adjustedWeight=0 → không bao giờ được chọn
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_WhenOnlyOnePetHasPositiveWeight_ShouldReturnThatPet()
        {
            var (service, repoMock) = CreateService();
            var petWithWeight = MakeSetPet(petId: 1, dropRate: 0.50, name: "Bulbasaur");
            var petZeroWeight = MakeSetPet(petId: 2, dropRate: 0.00, name: "ZeroPet");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([petWithWeight, petZeroWeight]);

            // Chạy 20 lần để đảm bảo tính deterministic
            for (int i = 0; i < 20; i++)
            {
                var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: 0);
                result.Should().NotBeNull();
                result!.Id.Should().Be(1,
                    because: "only Bulbasaur (DropRate=0.5) has positive weight; ZeroPet always has weight=0");
            }
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 5: Tất cả DropRate = 0 → totalWeight ≤ 0 → fallback về pet đầu tiên
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_WhenAllDropRatesAreZero_ShouldFallbackToFirstPet()
        {
            var (service, repoMock) = CreateService();
            var firstPet  = MakeSetPet(petId: 1, dropRate: 0.0, name: "FirstPet");
            var secondPet = MakeSetPet(petId: 2, dropRate: 0.0, name: "SecondPet");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([firstPet, secondPet]);

            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: 0);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1,
                because: "totalWeight=0 triggers fallback to the first pet in the list");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 6: Kết quả phải thuộc danh sách pet của set (không thể ra ngoài tập)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_ResultShouldAlwaysBelongToConfiguredSet()
        {
            var (service, repoMock) = CreateService();
            var configuredPetIds = new HashSet<int> { 10, 20, 30 };
            var setPets = configuredPetIds.Select(id => MakeSetPet(id, dropRate: 0.33)).ToList();
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync(setPets);

            for (int i = 0; i < 50; i++)
            {
                var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: i);
                result.Should().NotBeNull();
                configuredPetIds.Should().Contain(result!.Id,
                    because: "returned pet must be one of the configured reward pets for the set");
            }
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 7: Milestone boundaries không ném exception, trả về pet hợp lệ
        // Dùng single pet để đảm bảo deterministic ở mỗi ngưỡng milestone
        // ────────────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0,  "Milestone < 5: multiplier = 1×")]
        [InlineData(4,  "Milestone < 5: boundary max")]
        [InlineData(5,  "Milestone < 10: multiplier = 5×")]
        [InlineData(9,  "Milestone < 10: boundary max")]
        [InlineData(10, "Milestone < 20: multiplier = 10×")]
        [InlineData(19, "Milestone < 20: boundary max")]
        [InlineData(20, "Milestone < 50: multiplier = 20×")]
        [InlineData(49, "Milestone < 50: boundary max")]
        [InlineData(50, "Milestone ≥ 50: multiplier = 50×")]
        [InlineData(99, "Milestone ≥ 50: high value")]
        public async Task GetRandomPetBySetIdAsync_AtEachMilestoneBoundary_ShouldReturnPetWithoutThrowing(
            int milestone, string reason)
        {
            var (service, repoMock) = CreateService();
            var rarePet = MakeSetPet(petId: 1, dropRate: 0.05, name: "RarePet");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([rarePet]);

            var act = async () => await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: milestone);

            await act.Should().NotThrowAsync(because: reason);
            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: milestone);
            result.Should().NotBeNull(because: reason);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 8: Common pet (DropRate > 0.1) KHÔNG bị boost bởi milestone
        // Setup: chỉ có 1 common pet với DropRate=0.5 → trả về nó ở mọi milestone
        // ────────────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(9)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task GetRandomPetBySetIdAsync_CommonPetWithHighDropRate_ShouldNotBeAffectedByMilestone(
            int milestone)
        {
            var (service, repoMock) = CreateService();
            var commonPet = MakeSetPet(petId: 5, dropRate: 0.50, name: "CommonPet");
            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(1, default))
                .ReturnsAsync([commonPet]);

            var result = await service.GetRandomPetBySetIdAsync(vocabularySetId: 1, milestone: milestone);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5,
                because: $"common pets (DropRate > 0.1) are never boosted regardless of milestone={milestone}");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 9: Statistical — milestone=50 boost khiến rare pet thắng nhiều hơn
        //
        // Setup:
        //   Pet A (common):  DropRate=0.90 → adjustedWeight KHÔNG bị boost
        //   Pet B (rare):    DropRate=0.10 → adjustedWeight ĐƯỢC boost theo milestone
        //
        // milestone=0 : weightA=0.90, weightB=0.10×1=0.10 → P(B)≈10%
        // milestone=50: weightA=0.90, weightB=0.10×50=5.0  → P(B)≈84.7%
        //
        // Chạy 1000 lần, kiểm tra:
        //   milestone=0  → rare pet xuất hiện ≤ 20% (expect ~10%)
        //   milestone=50 → rare pet xuất hiện ≥ 60% (expect ~85%)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRandomPetBySetIdAsync_HighMilestone_RarePetShouldAppearSignificantlyMoreOften()
        {
            var (service, repoMock) = CreateService();
            const int iterations = 1000;
            const int setId = 99;

            var commonPet = MakeSetPet(petId: 1, dropRate: 0.90, name: "Pidgey");  // không boost
            var rarePet   = MakeSetPet(petId: 2, dropRate: 0.10, name: "Mewtwo");  // boost theo milestone

            repoMock
                .Setup(r => r.GetPetsByVocabularySetIdAsync(setId, default))
                .ReturnsAsync([commonPet, rarePet]);

            // --- milestone=0: P(rare) ≈ 10% ---
            int rareCountLow = 0;
            for (int i = 0; i < iterations; i++)
            {
                var result = await service.GetRandomPetBySetIdAsync(setId, milestone: 0);
                if (result?.Id == 2) rareCountLow++;
            }

            // --- milestone=50: P(rare) ≈ 84.7% ---
            int rareCountHigh = 0;
            for (int i = 0; i < iterations; i++)
            {
                var result = await service.GetRandomPetBySetIdAsync(setId, milestone: 50);
                if (result?.Id == 2) rareCountHigh++;
            }

            // milestone=0: rare pet KHÔNG được boost, tỉ lệ phải thấp
            rareCountLow.Should().BeLessThan(200,
                because: $"at milestone=0, rare pet has no boost → expected ~10% (~100 times), got {rareCountLow}/1000");

            // milestone=50: rare pet được boost 50×, tỉ lệ phải cao
            rareCountHigh.Should().BeGreaterThan(600,
                because: $"at milestone=50, rare pet weight = 0.10×50=5.0 → expected ~85% (~850 times), got {rareCountHigh}/1000");

            // milestone=50 phải cho tỉ lệ cao hơn milestone=0 đáng kể
            rareCountHigh.Should().BeGreaterThan(rareCountLow * 3,
                because: "milestone=50 should produce at least 3× more rare pet appearances than milestone=0");
        }
    }
}
