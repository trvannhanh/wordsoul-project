using System.ComponentModel.DataAnnotations;

namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Lưu trữ các tham số thuật toán và cấu hình hệ thống (Catch Rate, EF của SM-2, hệ số XP, v.v.)
    /// để Admin có thể điều chỉnh qua giao diện thay vì hardcode.
    /// </summary>
    public class SystemConfiguration
    {
        [Key]
        [MaxLength(100)]
        public required string Key { get; set; } // e.g., "SrsMinEf", "CatchRatePenalty", "BaseCatchRate_Common"

        [MaxLength(500)]
        public required string Value { get; set; } 

        [MaxLength(50)]
        public required string DataType { get; set; } // e.g., "Float", "Integer", "String", "Boolean"

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? LastUpdatedBy { get; set; } // Username của SuperAdmin sửa đổi
    }
}
