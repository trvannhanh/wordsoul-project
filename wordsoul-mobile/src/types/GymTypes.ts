export const GymStatus = {
  Locked: 0,
  Unlocked: 1,
  Defeated: 2,
} as const;

export type GymStatus = (typeof GymStatus)[keyof typeof GymStatus];
export type BattleStatus = 'InProgress' | 'Completed' | 'Abandoned';
export type QuestionType =
  | 'Flashcard'
  | 'FillInBlank'
  | 'MultipleChoice'
  | 'Listening';

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
  xpThreshold: number;
  vocabThreshold: number;
  requiredMemoryState: string;
  questionCount: number;
  passRatePercent: number;
  xpReward: number;
  status: GymStatus;
  totalAttempts: number;
  bestScore: number;
  defeatedAt?: string;
  cooldownEndsAt?: string;
  isOnCooldown: boolean;
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
  questionPrompt?: string;
}

export interface BattleSessionDto {
  id: number;
  gymLeaderId: number;
  status: BattleStatus;
  score: number;
  totalQuestions: number;
  correctAnswers: number;
  startedAt: string;
  completedAt?: string;
}
