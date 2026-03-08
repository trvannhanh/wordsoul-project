
namespace WordSoul.Application.DTOs.Pet
{
    /// <summary>
    /// Thông tin buff mà active pet mang lại cho người học.
    /// Chỉ dùng để display — backend chưa apply vào session.
    /// </summary>
    public class PetBuffDto
    {
        public int PetId { get; set; }

        // Buff identity
        public string BuffName { get; set; } = string.Empty;
        public string BuffDescription { get; set; } = string.Empty;
        public string BuffIcon { get; set; } = string.Empty;   // emoji dùng để display

        // Buff values (dùng để render UI, chưa apply logic)
        public double XpMultiplier { get; set; } = 1.0;        // 1.0 = không có bonus
        public double CatchRateBonus { get; set; } = 0.0;      // 0.08 = +8%
        public bool HasHintShield { get; set; } = false;       // 1 hint miễn phí/session
        public bool ReducePenalty { get; set; } = false;       // Sai không giảm catch rate

        
    }
}
