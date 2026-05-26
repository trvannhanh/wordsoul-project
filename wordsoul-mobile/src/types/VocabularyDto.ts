export interface VocabularyDto {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  pronunciationUrl: string | null;
  partOfSpeech: string;
  cefrLevel: string | null;
  description: string | null;
  exampleSentence: string | null;
  exampleSentenceAudioUrl: string | null;
}

export const VocabularyDifficultyLevelEnum = {
  Beginner: 0,
  Intermediate: 1,
  Advanced: 2,
} as const;

export type VocabularyDifficultyLevelEnum =
  (typeof VocabularyDifficultyLevelEnum)[keyof typeof VocabularyDifficultyLevelEnum];
