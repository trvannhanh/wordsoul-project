using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Tổng hợp giọng đọc bằng Azure AI Speech SDK và upload lên Azure Blob Storage.
    /// Audio format: MP3 (Audio16Khz32KBitRateMonoMp3).
    /// </summary>
    public class AzureSpeechService : IAzureSpeechService
    {
        private readonly ILogger<AzureSpeechService> _logger;
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly string _voiceName;
        private readonly BlobContainerClient _containerClient;
        private readonly string _blobBaseUrl;

        public AzureSpeechService(
            IConfiguration configuration,
            ILogger<AzureSpeechService> logger)
        {
            _logger = logger;
            _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"]
                ?? throw new InvalidOperationException("AzureSpeech:SubscriptionKey is not configured.");
            _region = configuration["AzureSpeech:Region"]
                ?? throw new InvalidOperationException("AzureSpeech:Region is not configured.");
            _voiceName = configuration["AzureSpeech:VoiceName"] ?? "en-US-AndrewNeural";

            var storageConn = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
            var containerName = configuration["AzureStorage:ContainerName"] ?? "vocab-audio";

            _containerClient = new BlobContainerClient(storageConn, containerName);

            // Build public base URL: https://{account}.blob.core.windows.net/{container}
            var blobServiceClient = new BlobServiceClient(storageConn);
            _blobBaseUrl = $"{blobServiceClient.Uri.AbsoluteUri.TrimEnd('/')}/{containerName}";
        }

        public async Task<string?> SynthesizeAndUploadAsync(
            string text,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                _logger.LogDebug("Synthesizing speech for: '{Text}' → {BlobName}", text, blobName);

                var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                speechConfig.SpeechSynthesisVoiceName = _voiceName;
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                byte[] audioData;

                var memSpeechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                memSpeechConfig.SpeechSynthesisVoiceName = _voiceName;
                memSpeechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                using var synthesizer2 = new SpeechSynthesizer(memSpeechConfig, null);
                using var result = await synthesizer2.SpeakTextAsync(text);

                if (result.Reason != ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.LogWarning("Speech synthesis failed for '{Text}': {Reason} — {Details}",
                        text, result.Reason, result.Reason.ToString());
                    return null;
                }

                audioData = result.AudioData;
                _logger.LogDebug("Audio synthesized: {Bytes} bytes for '{Text}'", audioData.Length, text);

                // Upload lên Azure Blob Storage
                await _containerClient.CreateIfNotExistsAsync(
                    PublicAccessType.Blob, cancellationToken: cancellationToken);

                var blobClient = _containerClient.GetBlobClient(blobName);

                using var stream = new MemoryStream(audioData);
                await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);

                var publicUrl = $"{_blobBaseUrl}/{blobName}";
                _logger.LogInformation("Audio uploaded: {Url}", publicUrl);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzureSpeechService failed for text='{Text}', blob='{BlobName}'", text, blobName);
                return null;
            }
        }
    }
}
