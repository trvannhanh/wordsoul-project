using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    /// <summary>
    /// Tính toán buff của active pet dựa trên Type, Rarity và Level.
    /// Buff chỉ dùng để display tại thời điểm này — chưa apply vào session logic.
    /// </summary>
    public class PetBuffService : IPetBuffService
    {
        private readonly IUnitOfWork _uow;

        // XP cố định mỗi level (khớp với UpgradePetForUserAsync)
        private const int XpPerLevel = 100;

        // Hệ số nhân buff theo Rarity
        private static readonly Dictionary<PetRarity, double> RarityMultiplier = new()
        {
            { PetRarity.Common,    1.0 },
            { PetRarity.Uncommon,  1.2 },
            { PetRarity.Rare,      1.4 },
            { PetRarity.Epic,      1.7 },
            { PetRarity.Legendary, 2.0 },
        };

        public PetBuffService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PetBuffDto?> GetActivePetBuffAsync(int userId, CancellationToken ct = default)
        {
            // Lấy active pet của user (UserOwnedPet)
            var activedPet = await _uow.UserOwnedPet.GetActivePetByUserIdAsync(userId, ct);
            if (activedPet == null) return null;

            // Tính buff
            var rarityMult = RarityMultiplier.GetValueOrDefault(activedPet.Rarity, 1.0);
            var (buffName, buffIcon, buffDesc, xpMult, catchBonus, hintShield, reducePenalty)
                = CalculateBuff(activedPet.Type, rarityMult);

            if (activedPet.SecondaryType.HasValue)
            {
                var (secName, secIcon, secDesc, secXp, secCatch, secHint, secReduce)
                    = CalculateBuff(activedPet.SecondaryType.Value, rarityMult);

                if (buffName != secName)
                {
                    buffName = $"{buffName} & {secName}";
                    buffIcon = $"{buffIcon}{secIcon}";
                    
                    if (buffDesc != secDesc)
                    {
                        buffDesc = $"{buffDesc}, {secDesc}";
                    }
                    
                    xpMult = 1.0 + (xpMult - 1.0) + (secXp - 1.0);
                    catchBonus += secCatch;
                    hintShield = hintShield || secHint;
                    reducePenalty = reducePenalty || secReduce;
                }
            }

            return new PetBuffDto
            {
                PetId = activedPet.Id,
                BuffName = buffName,
                BuffDescription = buffDesc,
                BuffIcon = buffIcon,
                XpMultiplier = xpMult,
                CatchRateBonus = catchBonus,
                HasHintShield = hintShield,
                ReducePenalty = reducePenalty
            };
        }

        /// <summary>
        /// Trả về (buffName, icon, description, xpMult, catchBonus, hintShield, reducePenalty)
        /// theo PetType, nhân với hệ số Rarity.
        /// </summary>
        private static (string, string, string, double, double, bool, bool) CalculateBuff(
            PetType type,
            double rarityMult)
        {
            return type switch
            {
                PetType.Fire => (
                    "Blaze",
                    "🔥",
                    $"+{FormatPct(0.10 * rarityMult)} XP mỗi phiên học",
                    1.0 + 0.10 * rarityMult,
                    0.0, false, false),

                PetType.Water => (
                    "Torrent",
                    "💧",
                    $"+{FormatPct(0.05 * rarityMult)} tỉ lệ bắt Pokémon",
                    1.0,
                    0.05 * rarityMult, false, false),

                PetType.Electric => (
                    "Static",
                    "⚡",
                    $"+{FormatPct(0.08 * rarityMult)} tỉ lệ bắt Pokémon",
                    1.0,
                    0.08 * rarityMult, false, false),

                PetType.Grass => (
                    "Overgrow",
                    "🌿",
                    $"+{FormatPct(0.05 * rarityMult)} tỉ lệ bắt Pokémon",
                    1.0,
                    0.05 * rarityMult, false, false),

                PetType.Psychic => (
                    "Telepathy",
                    "🔮",
                    "1 gợi ý miễn phí mỗi phiên học",
                    1.0,
                    0.0, true, false),

                PetType.Rock => (
                    "Sturdy",
                    "🪨",
                    "Trả lời sai không giảm tỉ lệ bắt",
                    1.0,
                    0.0, false, true),

                PetType.Dragon => (
                    "Multiscale",
                    "🐉",
                    $"+{FormatPct(0.20 * rarityMult)} XP — chế độ thách thức",
                    1.0 + 0.20 * rarityMult,
                    0.0, false, false),

                PetType.Fairy => (
                    "Pixilate",
                    "✨",
                    $"+{FormatPct(0.10 * rarityMult)} XP, +{FormatPct(0.05 * rarityMult)} tỉ lệ bắt",
                    1.0 + 0.10 * rarityMult,
                    0.05 * rarityMult, false, false),

                // Normal, Fighting, Ghost, Dark, Poison, Steel, Ice, Flying, ...
                _ => (
                    "Steadfast",
                    "⭐",
                    $"+{FormatPct(0.05 * rarityMult)} XP mỗi phiên học",
                    1.0 + 0.05 * rarityMult,
                    0.0, false, false),
            };
        }

        private static string FormatPct(double value)
            => $"{Math.Round(value * 100, 0):0}%";
    }
}