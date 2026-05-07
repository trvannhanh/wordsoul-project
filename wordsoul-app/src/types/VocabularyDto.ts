export interface VocabularyDto{
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  pronunciationUrl: string | null;       // URL audio MP3 of the word
  partOfSpeech: string;
  cefrLevel: string | null;              // A1 | A2 | B1 | B2 | C1 | C2
  description: string | null;           // English definition
  exampleSentence: string | null;       // Example sentence (was 'example')
  exampleSentenceAudioUrl: string | null; // URL audio MP3 of example sentence
}

export interface AdminVocabularyDto{
  id: number,
  word: string;
  meaning: string;
  pronunciation: string,
  partOfSpeech: string,
  cEFRLevel: string,
  description: string,
  exampleSentence: string,
  imageUrl: string,
  pronunciationUrl: string
}



export interface CreateVocabularyDto{
  word: string,
  meaning: string,
  pronunciation: string,
  partOfSpeech: string;
  cEFRLevel: string;
  description: string,
  exampleSentence: string,
  imageFile: string,
  pronunciationUrl: string
}

export interface SearchVocabularyDto {
  words: string[];
}

export const VocabularyDifficultyLevelEnum = {
  Beginner: 0,
  Intermediate: 1,
  Advanced: 2
} as const;

export type VocabularyDifficultyLevelEnum = typeof VocabularyDifficultyLevelEnum[keyof typeof VocabularyDifficultyLevelEnum];