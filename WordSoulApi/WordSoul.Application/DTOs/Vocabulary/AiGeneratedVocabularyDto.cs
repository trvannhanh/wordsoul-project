namespace WordSoul.Application.DTOs.Vocabulary
{
    /// <summary>
    /// DTO nhận kết quả từ Gemini AI cho một từ vựng.
    /// </summary>
    public class AiGeneratedVocabularyDto
    {
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";         // Nghĩa tiếng Việt
        public string Pronunciation { get; set; } = "";   // IPA, e.g. /wɜːrd/
        public string PartOfSpeech { get; set; } = "";    // "noun" | "verb" | "adjective" | ...
        public string CefrLevel { get; set; } = "";       // "A1" | "A2" | "B1" | "B2" | "C1" | "C2"
        public string Description { get; set; } = "";     // Định nghĩa tiếng Anh
        public string ExampleSentence { get; set; } = ""; // Câu ví dụ
    }
}
