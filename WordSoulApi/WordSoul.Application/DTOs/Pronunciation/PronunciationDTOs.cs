using System;
using System.Collections.Generic;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Pronunciation
{
    public class PronunciationAssessResponse
    {
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        public PronunciationResult Result { get; set; }
        public string ResultLabel { get; set; } = "";
        public int XpAwarded { get; set; }
        public double PetXpMultiplier { get; set; }
        public List<PhonemeResultDto> Phonemes { get; set; } = [];
    }

    public class PhonemeResultDto
    {
        public string Phoneme { get; set; } = "";
        public double AccuracyScore { get; set; }
        public string ResultLabel { get; set; } = "";
    }

    public class PronunciationWordDto
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? IpaTranscription { get; set; }
        public string? PronunciationUrl { get; set; }
        public string? ExampleSentence { get; set; }
        public string MemoryState { get; set; } = "";
        public int PronunciationWrongCount { get; set; }
        public DateTime? LastPronunciationAt { get; set; }
    }

    public class PronunciationHistoryDto
    {
        public DateTime AttemptTime { get; set; }
        public double PronunciationScore { get; set; }
        public string Result { get; set; } = "";
        public int XpAwarded { get; set; }
    }
}
