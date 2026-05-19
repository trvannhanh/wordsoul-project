using System.ComponentModel.DataAnnotations;

namespace WordSoul.Application.DTOs.Admin
{
    public class AdjustBalanceDto
    {
        /// <summary>Cộng/trừ XP. Dương = cộng, âm = trừ. 0 = không đổi.</summary>
        public int XpDelta { get; set; }

        /// <summary>Cộng/trừ AP.</summary>
        public int ApDelta { get; set; }

        /// <summary>Cộng/trừ HintBalance. Clamp về 0 nếu kết quả âm.</summary>
        public int HintDelta { get; set; }

        /// <summary>Lý do điều chỉnh — bắt buộc để ghi audit log.</summary>
        [Required]
        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AdjustBalanceResultDto
    {
        public int UserId { get; set; }
        public int NewXP { get; set; }
        public int NewAP { get; set; }
        public int NewHintBalance { get; set; }
    }
}
