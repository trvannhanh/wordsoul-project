export interface VocabularyDto{
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  partOfSpeech: string;
  example: string | null;
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