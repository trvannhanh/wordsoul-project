using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Dịch vụ đánh giá phát âm thông qua Azure Pronunciation Assessment API.
    /// Dùng chung cấu hình AzureSpeech:SubscriptionKey và AzureSpeech:Region với IAzureSpeechService.
    /// </summary>
    public interface IAzurePronunciationService
    {
        /// <summary>
        /// Đánh giá phát âm: nhận WAV stream (PCM 16kHz mono) và text tham chiếu, trả về kết quả chi tiết.
        /// </summary>
        Task<PronunciationAssessmentOutput> AssessAsync(
            Stream wavStream,
            string referenceText,
            CancellationToken ct = default);

        /// <summary>
        /// Convert audio bytes từ WebM/Opus (MediaRecorder) sang WAV PCM 16kHz mono.
        /// </summary>
        byte[] ConvertWebmToWav(byte[] webmBytes);
    }

    public class PronunciationAssessmentOutput
    {
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        /// <summary>Score tổng hợp từ Azure (0-100)</summary>
        public double PronunciationScore { get; set; }
        public List<PhonemeAssessmentDetail> Phonemes { get; set; } = [];
        /// <summary>JSON raw từ Azure để debug</summary>
        public string? RawJson { get; set; }

        /// <summary>
        /// Tính PronunciationResult từ PronunciationScore.
        /// Perfect >= 80, NearMiss 50-79, Wrong < 50
        /// </summary>
        public PronunciationResult ToResult() => PronunciationScore switch
        {
            >= 80 => PronunciationResult.Perfect,
            >= 50 => PronunciationResult.NearMiss,
            _     => PronunciationResult.Wrong
        };
    }

    public class PhonemeAssessmentDetail
    {
        public string Phoneme { get; set; } = "";
        public double AccuracyScore { get; set; }
        /// <summary>Kết quả đánh giá phoneme riêng lẻ</summary>
        public PronunciationResult Result => AccuracyScore switch
        {
            >= 80 => PronunciationResult.Perfect,
            >= 50 => PronunciationResult.NearMiss,
            _     => PronunciationResult.Wrong
        };
    }
}
