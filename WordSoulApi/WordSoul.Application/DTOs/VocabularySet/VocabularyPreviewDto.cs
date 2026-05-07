using System.ComponentModel.DataAnnotations;

namespace WordSoul.Application.DTOs.VocabularySet
{
    public class AiPreviewRequestDto
    {
        [Required(ErrorMessage = "At least one word is required")]
        public List<string> Words { get; set; } = [];

        /// <summary>Nếu false, bỏ qua bước gọi Gemini – các từ thiếu sẽ để trống cho user tự điền.</summary>
        public bool UseAi { get; set; } = true;
    }

    public class VocabularyPreviewDto
    {
        public int? Id { get; set; } // Có ID nếu từ này đã tồn tại trong DB
        public bool IsExisting { get; set; } // true: Đã có trong DB, false: Mới sinh bằng AI
        public bool IsAiGenerated { get; set; } // true: Được AI sinh thành công, false: Sinh lỗi/User tự điền
        
        [Required(ErrorMessage = "Word is required")]
        public string Word { get; set; } = "";
        
        public string? Meaning { get; set; }
        
        public string? Pronunciation { get; set; }
        
        public string? PartOfSpeech { get; set; }
        
        public string? CefrLevel { get; set; }
        public string? Description { get; set; }
        public string? ExampleSentence { get; set; }
    }
}
