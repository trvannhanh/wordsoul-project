export interface LearningSessionDto {
  id: number;
  vocabularyIds: number[];
  isCompleted: boolean;
  petId?: number;
  buffPetId?: number;
  catchRate?: number;
  currentCorrectAnswered?: number;

  buffName?: string;
  buffDescription?: string;
  buffIcon?: string;
  petXpMultiplier?: number;
  petCatchBonus?: number;
  petHintShield?: boolean;
  petReducePenalty?: boolean;
}

export interface QuizQuestionDto {
  vocabularyId: number;
  questionType: QuestionTypeEnum;
  word: string;
  meaning?: string;
  partOfSpeech?: string;
  cefrLevel?: string;
  pronunciation?: string;
  imageUrl?: string;
  description?: string;
  options?: string[];
  pronunciationUrl?: string;
  isRetry?: boolean; // Thêm thuộc tính isRetry
}


export interface AnswerRequestDto {
  vocabularyId: number;
  questionType: QuestionTypeEnum;
  answer: string;
  responseTimeSeconds: number;
  hintCount: number;
}

export interface AnswerResponseDto {
  isCorrect: boolean;
  correctAnswer: string;
  attemptNumber: number;
  newLevel: number; // 0-3: Flashcard → Listening
  isVocabularyCompleted: boolean;
}


export interface CompleteLearningSessionResponseDto {
  xpEarned: number;
  isPetRewardGranted: boolean;
  isPetAlreadyOwned: boolean;
  petId?: number;
  petName?: string;
  description?: string;
  imageUrl?: string;
  petRarity?: string;
  petType?: string;
  message: string;
}

export interface CompleteReviewSessionResponseDto {
  xpEarned: number;
  apEarned: number;
  message: string;
}

export const QuestionTypeEnum = {
  Flashcard: 0,
  FillInBlank: 1,
  MultipleChoice: 2,
  Listening: 3,
} as const;

export type QuestionTypeEnum = typeof QuestionTypeEnum[keyof typeof QuestionTypeEnum];
