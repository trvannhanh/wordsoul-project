using System.ComponentModel.DataAnnotations;

namespace WordSoul.Domain.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [MaxLength(10)]
        public required string Method { get; set; }
        [MaxLength(500)]
        public required string Path { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        
        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; }
        
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        
        [MaxLength(50)]
        public string? UserId { get; set; } // string hoặc int tuỳ vào hệ thống, nhưng token có thể cung cấp string Id.
    }
}
