using FluentAssertions;
using Moq;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services
{
    /// <summary>
    /// Unit tests cho PetBuffService.GetActivePetBuffAsync
    ///
    /// Công thức buff:
    ///   rarityMult: Common=1.0 | Uncommon=1.2 | Rare=1.4 | Epic=1.7 | Legendary=2.0
    ///   XP types  : Fire  → 1.0 + 0.10×rarity | Dragon → 1.0 + 0.20×rarity | Fairy → 1.0 + 0.10×rarity
    ///               Default (Normal, Ice, ...) → 1.0 + 0.05×rarity
    ///   Catch types: Water / Grass → 0.05×rarity | Electric → 0.08×rarity | Fairy → 0.05×rarity
    ///   Flags       : Psychic → HasHintShield=true | Rock → ReducePenalty=true
    ///   Dual type   : XpBonus cộng dồn, CatchBonus cộng dồn, flags OR
    /// </summary>
    public class PetBuffServiceTests
    {
        // ────────────────────────────────────────────────────────────────────────────
        // Helper
        // ────────────────────────────────────────────────────────────────────────────

        private static (PetBuffService service, Mock<IUserOwnedPetRepository> repoMock)
            CreateService()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IUserOwnedPetRepository>();
            uowMock.SetupGet(x => x.UserOwnedPet).Returns(repoMock.Object);
            var service = new PetBuffService(uowMock.Object);
            return (service, repoMock);
        }

        private static Pet MakePet(
            PetType type,
            PetRarity rarity,
            PetType? secondaryType = null,
            int id = 1)
            => new()
            {
                Id = id,
                Name = $"TestPet_{type}",
                Type = type,
                Rarity = rarity,
                SecondaryType = secondaryType,
            };

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 1: Không có active pet → trả về null
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_WhenNoPetActive_ShouldReturnNull()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pet?)null);

            var result = await service.GetActivePetBuffAsync(userId: 1);

            result.Should().BeNull();
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 2: Fire type → XP multiplier
        // XpMultiplier = 1.0 + 0.10 × rarityMult
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_FireType_Common_ShouldApplyXpMultiplier1_1()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Fire, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(1.10, 0.0001);
            buff.CatchRateBonus.Should().BeApproximately(0.0, 0.0001);
            buff.HasHintShield.Should().BeFalse();
            buff.ReducePenalty.Should().BeFalse();
            buff.PetId.Should().Be(1);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 3: Dragon type → cao nhất XP multiplier (0.20 × rarity)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_DragonType_Common_ShouldApplyHighXpMultiplier1_2()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Dragon, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(1.20, 0.0001,
                because: "Dragon gives +20% XP at Common rarity");
            buff.CatchRateBonus.Should().BeApproximately(0.0, 0.0001);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 4: Water type → CatchRate bonus (0.05 × rarity), không có XP buff
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_WaterType_ShouldApplyCatchRateBonus_NotXp()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Water, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(1.0, 0.0001,
                because: "Water type does not give XP bonus");
            buff.CatchRateBonus.Should().BeApproximately(0.05, 0.0001,
                because: "Water type gives +5% catch rate at Common rarity");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 5: Electric type → CatchRate cao hơn Water (0.08 × rarity)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_ElectricType_ShouldApplyHigherCatchRateThanWater()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Electric, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.CatchRateBonus.Should().BeApproximately(0.08, 0.0001,
                because: "Electric type gives +8% catch rate (higher than Water's 5%)");
            buff.XpMultiplier.Should().BeApproximately(1.0, 0.0001);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 6: Grass type → CatchRate bằng Water (0.05)
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_GrassType_ShouldApplySameCatchRateAsWater()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Grass, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff!.CatchRateBonus.Should().BeApproximately(0.05, 0.0001);
            buff.XpMultiplier.Should().BeApproximately(1.0, 0.0001);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 7: Psychic type → HasHintShield = true
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_PsychicType_ShouldEnableHintShield()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Psychic, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.HasHintShield.Should().BeTrue(
                because: "Psychic type grants free hint shield per session");
            buff.ReducePenalty.Should().BeFalse();
            buff.XpMultiplier.Should().BeApproximately(1.0, 0.0001);
            buff.CatchRateBonus.Should().BeApproximately(0.0, 0.0001);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 8: Rock type → ReducePenalty = true
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_RockType_ShouldEnableReducePenalty()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Rock, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.ReducePenalty.Should().BeTrue(
                because: "Rock type prevents catch rate reduction on wrong answers");
            buff.HasHintShield.Should().BeFalse();
            buff.XpMultiplier.Should().BeApproximately(1.0, 0.0001);
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 9: Fairy type → cả XP buff + CatchRate buff
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_FairyType_ShouldApplyBothXpAndCatchRateBuff()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Fairy, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(1.10, 0.0001,
                because: "Fairy gives +10% XP (same rate as Fire)");
            buff.CatchRateBonus.Should().BeApproximately(0.05, 0.0001,
                because: "Fairy also gives +5% catch rate");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 10: Default type (Normal) → nhỏ nhất, +5% XP
        // ────────────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(PetType.Normal)]
        [InlineData(PetType.Ice)]
        [InlineData(PetType.Fighting)]
        [InlineData(PetType.Ghost)]
        [InlineData(PetType.Dark)]
        public async Task GetActivePetBuffAsync_DefaultTypes_ShouldApplySmallXpBonus(PetType type)
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(type, PetRarity.Common));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(1.05, 0.0001,
                because: $"{type} type falls into default branch giving +5% XP at Common rarity");
            buff.CatchRateBonus.Should().BeApproximately(0.0, 0.0001);
            buff.HasHintShield.Should().BeFalse();
            buff.ReducePenalty.Should().BeFalse();
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 11: Rarity scaling — Legendary nhân đôi buff so với Common
        // Kiểm tra với Fire type: Common→1.1, Uncommon→1.12, Rare→1.14, Epic→1.17, Legendary→1.2
        // ────────────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(PetRarity.Common,    1.10)]
        [InlineData(PetRarity.Uncommon,  1.12)]
        [InlineData(PetRarity.Rare,      1.14)]
        [InlineData(PetRarity.Epic,      1.17)]
        [InlineData(PetRarity.Legendary, 1.20)]
        public async Task GetActivePetBuffAsync_FireType_AllRarities_ShouldScaleXpMultiplierCorrectly(
            PetRarity rarity, double expectedXpMult)
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Fire, rarity));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.XpMultiplier.Should().BeApproximately(expectedXpMult, 0.0001,
                because: $"Fire type with {rarity} rarity should give XpMultiplier = 1.0 + 0.10 × {(int)rarity * 0.0 + expectedXpMult - 1.0:F2}");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 12: Dual type — buff từ cả hai type được cộng dồn
        // Fire (primary) + Water (secondary): XP=1.1, CatchRate=0.05, cả hai được merge
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_DualType_FireAndWater_ShouldMergeXpAndCatchRate()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Fire, PetRarity.Common, secondaryType: PetType.Water));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            // XP từ Fire: 1.0 + 0.10 = 1.1; Water không có XP bonus → combined = 1.1
            buff!.XpMultiplier.Should().BeApproximately(1.10, 0.0001,
                because: "Fire contributes +10% XP, Water contributes +0% XP → total 1.1");
            // CatchRate từ Water: 0.05; Fire không có catch bonus → combined = 0.05
            buff.CatchRateBonus.Should().BeApproximately(0.05, 0.0001,
                because: "Water contributes +5% catch rate, Fire contributes 0 → total 0.05");
            buff.BuffName.Should().Contain("Blaze").And.Contain("Torrent",
                because: "Dual-type buff name should include both primary and secondary buff names");
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TEST 13: Dual type — Psychic + Rock → HasHintShield=true AND ReducePenalty=true
        // ────────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivePetBuffAsync_DualType_PsychicAndRock_ShouldGrantBothFlags()
        {
            var (service, repoMock) = CreateService();
            repoMock
                .Setup(r => r.GetActivePetByUserIdAsync(1, default))
                .ReturnsAsync(MakePet(PetType.Psychic, PetRarity.Common, secondaryType: PetType.Rock));

            var buff = await service.GetActivePetBuffAsync(userId: 1);

            buff.Should().NotBeNull();
            buff!.HasHintShield.Should().BeTrue(
                because: "Psychic primary type grants hint shield");
            buff.ReducePenalty.Should().BeTrue(
                because: "Rock secondary type grants reduce penalty");
            buff.XpMultiplier.Should().BeApproximately(1.0, 0.0001);
        }
    }
}
