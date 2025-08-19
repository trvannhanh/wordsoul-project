using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyRepository _vocabularyRepository;

        public VocabularyService(IVocabularyRepository vocabularyRepository)
        {
            _vocabularyRepository = vocabularyRepository;
        }

        public async Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync()
        {
            var vocabularies = await _vocabularyRepository.GetAllVocabulariesAsync();
            var vocabularyDtos = new List<VocabularyDto>();
            foreach (var vocabulary in vocabularies)
            {
                vocabularyDtos.Add(new VocabularyDto
                {
                    Id = vocabulary.Id,
                    Word = vocabulary.Word,
                    Meaning = vocabulary.Meaning,
                    PartOfSpeech = vocabulary.PartOfSpeech,
                    CEFRLevel = vocabulary.CEFRLevel,
                    Description = vocabulary.Description,
                    ExampleSentence = vocabulary.ExampleSentence,
                    ImageUrl = vocabulary.ImageUrl,
                    PronunciationUrl = vocabulary.PronunciationUrl
                });
            }
            return vocabularyDtos;
        }

        public async Task<VocabularyDto?> GetVocabularyByIdAsync(int id)
        {
            var vocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(id);
            if (vocabulary == null) return null;
            return new VocabularyDto
            {
                Id = vocabulary.Id,
                Word = vocabulary.Word,
                Meaning = vocabulary.Meaning,
                PartOfSpeech = vocabulary.PartOfSpeech,
                CEFRLevel = vocabulary.CEFRLevel,
                Description = vocabulary.Description,
                ExampleSentence = vocabulary.ExampleSentence,
                ImageUrl = vocabulary.ImageUrl,
                PronunciationUrl = vocabulary.PronunciationUrl
            };
        }

        public async Task<VocabularyDto> CreateVocabularyAsync(VocabularyDto vocabularyDto)
        {
            var vocabulary = new Vocabulary
            {
                Word = vocabularyDto.Word,
                Meaning = vocabularyDto.Meaning,
                PartOfSpeech = vocabularyDto.PartOfSpeech,
                CEFRLevel = vocabularyDto.CEFRLevel,
                Description = vocabularyDto.Description,
                ExampleSentence = vocabularyDto.ExampleSentence,
                ImageUrl = vocabularyDto.ImageUrl,
                PronunciationUrl = vocabularyDto.PronunciationUrl
            };
            var createdVocabulary = await _vocabularyRepository.CreateVocabularyAsync(vocabulary);
            return new VocabularyDto
            {
                Id = createdVocabulary.Id,
                Word = createdVocabulary.Word,
                Meaning = createdVocabulary.Meaning,
                PartOfSpeech = createdVocabulary.PartOfSpeech,
                CEFRLevel = createdVocabulary.CEFRLevel,
                Description = createdVocabulary.Description,
                ExampleSentence = createdVocabulary.ExampleSentence,
                ImageUrl = createdVocabulary.ImageUrl,
                PronunciationUrl = createdVocabulary.PronunciationUrl
            };
        }

        public async Task<VocabularyDto> UpdateVocabularyAsync(int id, VocabularyDto vocabularyDto)
        {
            var existingVocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(id);
            if (existingVocabulary == null)
            {
                throw new KeyNotFoundException($"Vocabulary with ID {id} not found.");
            }

            existingVocabulary.Word = vocabularyDto.Word;
            existingVocabulary.Meaning = vocabularyDto.Meaning;
            existingVocabulary.PartOfSpeech = vocabularyDto.PartOfSpeech;
            existingVocabulary.CEFRLevel = vocabularyDto.CEFRLevel;
            existingVocabulary.Description = vocabularyDto.Description;
            existingVocabulary.ExampleSentence = vocabularyDto.ExampleSentence;
            existingVocabulary.ImageUrl = vocabularyDto.ImageUrl;
            existingVocabulary.PronunciationUrl = vocabularyDto.PronunciationUrl;

            var updatedVocabulary = await _vocabularyRepository.UpdateVocabularyAsync(existingVocabulary);
            return new VocabularyDto
            {
                Id = updatedVocabulary.Id,
                Word = updatedVocabulary.Word,
                Meaning = updatedVocabulary.Meaning,
                PartOfSpeech = updatedVocabulary.PartOfSpeech,
                CEFRLevel = updatedVocabulary.CEFRLevel,
                Description = updatedVocabulary.Description,
                ExampleSentence = updatedVocabulary.ExampleSentence,
                ImageUrl = updatedVocabulary.ImageUrl,
                PronunciationUrl = updatedVocabulary.PronunciationUrl
            };
        }

        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            return await _vocabularyRepository.DeleteVocabularyAsync(id);
        }

        public async Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto)
        {
            if (searchVocabularyDto == null || searchVocabularyDto.Words == null || !searchVocabularyDto.Words.Any())
                return new List<VocabularyDto>();


            var vocabularies = await _vocabularyRepository.GetVocabulariesByWordsAsync(searchVocabularyDto.Words);
            var vocabularyDtos = vocabularies.Select(v => new VocabularyDto
            {
                Id = v.Id,
                Word = v.Word,
                Meaning = v.Meaning,
                PartOfSpeech = v.PartOfSpeech,
                CEFRLevel = v.CEFRLevel,
                Description = v.Description,
                ExampleSentence = v.ExampleSentence,
                ImageUrl = v.ImageUrl,
                PronunciationUrl = v.PronunciationUrl
            }).ToList();

            return vocabularyDtos;
        }
    }
}