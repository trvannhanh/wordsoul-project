using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Gọi Gemini API để sinh metadata từ vựng tự động.
    /// Hỗ trợ batch processing và rate limit handling.
    /// </summary>
    public class GeminiAiService : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiAiService> _logger;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _batchSize;
        private readonly int _delayMs;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GeminiApi:ApiKey"]
                ?? throw new InvalidOperationException("GeminiApi:ApiKey is not configured.");
            _model = configuration["GeminiApi:Model"] ?? "gemini-2.5-flash";
            
            if (!int.TryParse(configuration["GeminiApi:BatchSize"], out _batchSize))
                _batchSize = 10;
            
            if (!int.TryParse(configuration["GeminiApi:DelayBetweenBatchesMs"], out _delayMs))
                _delayMs = 5000;
        }

        public async Task<List<AiGeneratedVocabularyDto>> GenerateVocabularyMetadataAsync(
            List<string> words,
            CancellationToken cancellationToken = default)
        {
            if (words == null || words.Count == 0)
                return [];

            var results = new List<AiGeneratedVocabularyDto>();
            var batches = words
                .Select((w, i) => new { Word = w, Index = i })
                .GroupBy(x => x.Index / _batchSize)
                .Select(g => g.Select(x => x.Word).ToList())
                .ToList();

            _logger.LogInformation(
                "GeminiAiService: processing {TotalWords} words in {BatchCount} batches (size={BatchSize})",
                words.Count, batches.Count, _batchSize);

            for (int batchIdx = 0; batchIdx < batches.Count; batchIdx++)
            {
                var batch = batches[batchIdx];
                _logger.LogDebug("Processing batch {Idx}/{Total}: [{Words}]",
                    batchIdx + 1, batches.Count, string.Join(", ", batch));

                try
                {
                    var batchResults = await ProcessBatchAsync(batch, cancellationToken);
                    results.AddRange(batchResults);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch {Idx} failed — skipping words: {Words}",
                        batchIdx + 1, string.Join(", ", batch));
                }

                // Delay giữa các batch để tránh rate limit (trừ batch cuối)
                if (batchIdx < batches.Count - 1)
                {
                    _logger.LogDebug("Waiting {DelayMs}ms before next batch...", _delayMs);
                    await Task.Delay(_delayMs, cancellationToken);
                }
            }

            _logger.LogInformation("GeminiAiService: completed. {Count}/{Total} words processed successfully.",
                results.Count, words.Count);

            return results;
        }

        private async Task<List<AiGeneratedVocabularyDto>> ProcessBatchAsync(
            List<string> words,
            CancellationToken cancellationToken)
        {
            var wordListJson = JsonSerializer.Serialize(words);
            
            var systemInstruction = """
                You are a high-precision linguistic API engine for "Vocamon." Your goal is to transform raw words into structured educational metadata.

                ### EXCLUSION CRITERIA (Strict):
                Before processing, filter the input list. Do NOT return data for:
                - Proper Nouns: Names of people, specific places, brands, or organizations (e.g., "John", "London", "Microsoft").
                - Non-dictionary strings: Gibberish, strings with numbers/symbols (e.g., "word123").
                - Abbreviations: Technical or non-standard ones unless globally recognized as common words.
                - Non-standard forms: Always convert words to their BASE FORM (Infinitive for verbs, Singular for nouns).

                ### LINGUISTIC RULES:
                1. WORD: Standard dictionary base form (lowercase).
                2. MEANING: Most common Vietnamese translation. Match the chosen 'partOfSpeech'.
                3. PRONUNCIATION: Standard IPA notation enclosed in slashes (e.g., /əˈfensɪv/).
                4. PART OF SPEECH: Strictly map to ONLY ONE of these exact values: "noun", "verb", "adjective", "adverb", "pronoun", "preposition", "conjunction", "interjection", "determiner", "auxiliaryverb", "particle".
                5. CEFR LEVEL: Assign based on Oxford/Cambridge standards (A1-C2).
                6. DESCRIPTION: Clear English definition (1-2 sentences). Use vocabulary suitable for the assigned CEFR level. Do NOT use the word itself in the description.
                7. EXAMPLE SENTENCE: A natural, complete English sentence using the word in the correct grammatical form.
                """;

            var userPrompt = $$$"""
                ### INPUT:
                Process the following JSON array of words: {{{wordListJson}}}

                ### OUTPUT FORMAT:
                - Return ONLY a valid JSON array of objects.
                - NO Markdown code blocks (no ```json).
                - NO conversational text.
                - Encoding: UTF-8.
                - If a word is excluded, omit it from the array.

                ### JSON SCHEMA:
                [
                  {
                    "word": "string",
                    "meaning": "string",
                    "pronunciation": "string",
                    "partOfSpeech": "string",
                    "cefrLevel": "string",
                    "description": "string",
                    "exampleSentence": "string"
                  }
                ]
                """;

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 8192
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error {Status}: {Body}", response.StatusCode, errorBody);
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseGeminiResponse(responseJson, words);
        }

        private List<AiGeneratedVocabularyDto> ParseGeminiResponse(string responseJson, List<string> requestedWords)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                // Lấy text từ candidates[0].content.parts[0].text
                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                // Làm sạch markdown code block nếu có
                text = text.Trim();
                if (text.StartsWith("```"))
                {
                    var firstNewline = text.IndexOf('\n');
                    var lastTriple = text.LastIndexOf("```");
                    if (firstNewline >= 0 && lastTriple > firstNewline)
                        text = text[(firstNewline + 1)..lastTriple].Trim();
                }

                var items = JsonSerializer.Deserialize<List<AiGeneratedVocabularyDto>>(text, _jsonOptions);
                if (items == null || items.Count == 0)
                {
                    _logger.LogWarning("Gemini returned empty array for words: {Words}", string.Join(", ", requestedWords));
                    return [];
                }

                _logger.LogDebug("Gemini returned {Count} items for {Expected} words", items.Count, requestedWords.Count);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response. Raw: {Response}", responseJson[..Math.Min(500, responseJson.Length)]);
                return [];
            }
        }
    }
}
