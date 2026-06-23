

namespace WordSoul.Domain.Enums
{
    public enum ConditionType
    {
        MasterWords,
        DailyStreak,
        CompletedSet,
        CatchedPets,
        GymDefeated,          // Dành riêng để track badge khi chinh phục Gym Leader
        PronunciationMastered // Đạt kết quả "Perfect" khi luyện phát âm X từ tổng cộng
    }
}
