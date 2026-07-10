using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Dịch vụ đánh giá phát âm sử dụng Azure Cognitive Services Pronunciation Assessment.
    /// Dùng chung AzureSpeech:SubscriptionKey và AzureSpeech:Region với AzureSpeechService (TTS).
    /// </summary>
    public class AzurePronunciationService : IAzurePronunciationService
    {
        private readonly ILogger<AzurePronunciationService> _logger;
        private readonly string _subscriptionKey;
        private readonly string _region;

        public AzurePronunciationService(
            IConfiguration configuration,
            ILogger<AzurePronunciationService> logger)
        {
            _logger = logger;
            _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"]
                ?? throw new InvalidOperationException("AzureSpeech:SubscriptionKey is not configured.");
            _region = configuration["AzureSpeech:Region"]
                ?? throw new InvalidOperationException("AzureSpeech:Region is not configured.");
        }

        public async Task<PronunciationAssessmentOutput> AssessAsync(
            Stream wavStream,
            string referenceText,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(referenceText))
                throw new ArgumentException("Reference text cannot be empty.", nameof(referenceText));

            try
            {
                _logger.LogDebug("Assessing pronunciation for: '{Text}'", referenceText);

                // Copy stream sang MemoryStream để đảm bảo seekable
                byte[] audioBytes;
                using (var ms = new MemoryStream())
                {
                    await wavStream.CopyToAsync(ms, ct);
                    audioBytes = ms.ToArray();
                }

                var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);

                // Cấu hình Pronunciation Assessment
                var pronunciationConfig = new PronunciationAssessmentConfig(
                    referenceText,
                    GradingSystem.HundredMark,
                    Granularity.Phoneme,
                    enableMiscue: false);

                using var audioStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1));
                audioStream.Write(audioBytes);
                audioStream.Close();

                using var audioConfig = AudioConfig.FromStreamInput(audioStream);
                using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

                pronunciationConfig.ApplyTo(recognizer);

                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.NoMatch || result.Reason == ResultReason.Canceled)
                {
                    _logger.LogWarning(
                        "Pronunciation assessment failed for '{Text}': Reason={Reason}",
                        referenceText, result.Reason);

                    // Trả về điểm 0 khi không nhận dạng được
                    return new PronunciationAssessmentOutput
                    {
                        AccuracyScore = 0,
                        FluencyScore = 0,
                        CompletenessScore = 0,
                        PronunciationScore = 0,
                        RawJson = $"{{\"error\": \"{result.Reason}\"}}"
                    };
                }

                var assessment = PronunciationAssessmentResult.FromResult(result);

                // Parse phoneme details
                var phonemes = new List<PhonemeAssessmentDetail>();
                var rawJson = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

                try
                {
                    using var doc = JsonDocument.Parse(rawJson);
                    if (doc.RootElement.TryGetProperty("NBest", out var nBest) && nBest.GetArrayLength() > 0)
                    {
                        var best = nBest[0];
                        if (best.TryGetProperty("Words", out var words))
                        {
                            foreach (var word in words.EnumerateArray())
                            {
                                if (!word.TryGetProperty("Phonemes", out var phonemeList)) continue;
                                foreach (var phoneme in phonemeList.EnumerateArray())
                                {
                                    phonemes.Add(new PhonemeAssessmentDetail
                                    {
                                        Phoneme = phoneme.TryGetProperty("Phoneme", out var p) ? p.GetString() ?? "" : "",
                                        AccuracyScore = phoneme.TryGetProperty("PronunciationAssessment", out var pa)
                                            && pa.TryGetProperty("AccuracyScore", out var score)
                                            ? score.GetDouble() : 0
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse phoneme details from Azure response");
                }

                var output = new PronunciationAssessmentOutput
                {
                    AccuracyScore = assessment.AccuracyScore,
                    FluencyScore = assessment.FluencyScore,
                    CompletenessScore = assessment.CompletenessScore,
                    PronunciationScore = assessment.PronunciationScore,
                    Phonemes = phonemes,
                    RawJson = rawJson
                };

                _logger.LogInformation(
                    "Pronunciation assessed: '{Text}' → Score={Score:F1} (Acc={Acc:F1}, Flu={Flu:F1}, Com={Com:F1})",
                    referenceText, output.PronunciationScore, output.AccuracyScore,
                    output.FluencyScore, output.CompletenessScore);

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzurePronunciationService failed for text='{Text}'", referenceText);
                throw;
            }
        }

        /// <summary>
        /// Convert WebM/Opus bytes (từ MediaRecorder) sang WAV PCM 16kHz mono.
        /// Dùng NAudio.Wave để decode + resample.
        /// </summary>
        public byte[] ConvertWebmToWav(byte[] webmBytes)
        {
            // NAudio không hỗ trợ WebM/Opus decode trực tiếp.
            // Dùng approach: đọc raw bytes qua RawSourceWaveStream với assumed format,
            // hoặc dùng FFMpegCore nếu muốn full support.
            //
            // Với các audio từ Chrome MediaRecorder (WebM/Opus), cần external decoder.
            // Hiện tại: nếu client gửi WAV (với MediaRecorder mimeType audio/wav hoặc sau convert),
            // thì giữ nguyên. Nếu gửi WebM, frontend cần convert trước hoặc dùng FFMpegCore.
            //
            // Phiên bản đơn giản: chỉ reformat sang WAV PCM nếu input đã là PCM
            using var inputMs = new MemoryStream(webmBytes);
            using var outputMs = new MemoryStream();

            // Attempt: nếu file có WAV header hợp lệ, đọc và resample sang 16kHz
            try
            {
                using var reader = new NAudio.Wave.WaveFileReader(inputMs);
                var targetFormat = new NAudio.Wave.WaveFormat(16000, 16, 1);

                if (reader.WaveFormat.SampleRate != 16000
                    || reader.WaveFormat.BitsPerSample != 16
                    || reader.WaveFormat.Channels != 1)
                {
                    using var resampler = new NAudio.Wave.MediaFoundationResampler(reader, targetFormat);
                    NAudio.Wave.WaveFileWriter.WriteWavFileToStream(outputMs, resampler);
                }
                else
                {
                    NAudio.Wave.WaveFileWriter.WriteWavFileToStream(outputMs, reader);
                }
            }
            catch
            {
                // Nếu không phải WAV hợp lệ, trả về nguyên bytes để Azure tự xử lý
                return webmBytes;
            }

            return outputMs.ToArray();
        }
    }
}
