// Types cho Gym Leader System

export const GymStatus = {
  Locked: 0,
  Unlocked: 1,
  Defeated: 2,
} as const;

export type GymStatus = typeof GymStatus[keyof typeof GymStatus];
export type BattleStatus = 'InProgress' | 'Completed' | 'Abandoned';
export type QuestionType = 'Flashcard' | 'FillInBlank' | 'MultipleChoice' | 'Listening';

export interface GymLeaderDto {
  id: number;
  gymOrder: number;
  name: string;
  title: string;
  description: string;
  avatarUrl?: string;
  badgeName: string;
  badgeImageUrl?: string;

  theme: string;
  requiredCefrLevel: string;

  // Điều kiện mở khóa
  xpThreshold: number;
  vocabThreshold: number;
  requiredMemoryState: string;

  // Battle config
  questionCount: number;
  passRatePercent: number;
  xpReward: number;

  // Trạng thái user
  status: GymStatus;
  totalAttempts: number;
  bestScore: number;
  defeatedAt?: string;
  cooldownEndsAt?: string;
  isOnCooldown: boolean;

  // Tiến trình hiện tại
  currentXp: number;
  currentVocabCount: number;
}

export interface BattleQuizQuestion {
  vocabularyId: number;
  questionType: QuestionType;
  word?: string;
  meaning?: string;
  pronunciation?: string;
  partOfSpeech?: string;
  cefrLevel?: string;
  imageUrl?: string;
  description?: string;
  options?: string[];
  pronunciationUrl?: string;
  isRetry: boolean;
  questionPrompt?: string;
}

export interface StartBattleResponseDto {
  battleSessionId: number;
  gymLeaderId: number;
  gymLeaderName: string;
  gymLeaderTitle: string;
  gymLeaderAvatarUrl?: string;
  totalQuestions: number;
  passRatePercent: number;
  questions: BattleQuizQuestion[];
}

export interface BattleAnswerDto {
  vocabularyId: number;
  answer: string;
  questionOrder: number;
  responseTimeMs: number;
}

export interface BattleAnswerResultDto {
  vocabularyId: number;
  word: string;
  meaning: string;
  userAnswer?: string;
  isCorrect: boolean;
  questionOrder: number;
}

export interface BattleResultDto {
  battleSessionId: number;
  gymLeaderId: number;
  gymLeaderName: string;

  isVictory: boolean;
  correctAnswers: number;
  totalQuestions: number;
  scorePercent: number;
  passRatePercent: number;

  xpEarned: number;
  badgeEarned: boolean;
  badgeName?: string;
  badgeImageUrl?: string;

  isOnCooldown: boolean;
  cooldownEndsAt?: string;

  newGymStatus: GymStatus;
  bestScore: number;

  answerResults: BattleAnswerResultDto[];
}
