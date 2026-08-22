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
  phase: QuestionPhaseEnum;
  revealsAnswer: boolean;
  countsAsRecall: boolean;
  word?: string;
  meaning?: string;
  partOfSpeech?: string;
  cefrLevel?: string;
  pronunciation?: string;
  imageUrl?: string;
  description?: string;
  options?: string[];
  hintOptionsToEliminate?: string[];
  hintText?: string;
  pronunciationUrl?: string;
  isRetry?: boolean;
  questionPrompt?: string;
}

export interface AnswerRequestDto {
  submissionId: string;
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
  newStageIndex: number;
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
  Flashcard: 'Flashcard',
  FillInBlank: 'FillInBlank',
  MultipleChoice: 'MultipleChoice',
  Listening: 'Listening',
} as const;

export type QuestionTypeEnum =
  (typeof QuestionTypeEnum)[keyof typeof QuestionTypeEnum];

export const QuestionPhaseEnum = {
  Study: 'Study',
  GuidedRecall: 'GuidedRecall',
  Recognition: 'Recognition',
  ProductiveRecall: 'ProductiveRecall',
  InitialRecall: 'InitialRecall',
  Feedback: 'Feedback',
  CorrectiveRecall: 'CorrectiveRecall',
} as const;

export type QuestionPhaseEnum =
  (typeof QuestionPhaseEnum)[keyof typeof QuestionPhaseEnum];
