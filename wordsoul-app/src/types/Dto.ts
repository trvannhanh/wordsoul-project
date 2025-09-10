export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface UserDto {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  isActive: boolean;
}

export interface RefreshTokenRequestDto {
  id: number;
  refreshToken: string;
}

export interface TokenResponseDto {
  accessToken: string;
  refreshToken: string;
}

export interface VocabularySet {
  id: number;
  title: string;
  description: string | null;
  theme: string;
  difficultyLevel: string;
  imageUrl?: string;
  isActive?: boolean;
  createdAt?: string;
}

export interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  partOfSpeech: string;
}

export interface VocabularySetDetail extends VocabularySet {
  vocabularies: Vocabulary[];
  totalVocabularies: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

export interface LearningSession {
  id: number;
  vocabularies: number[];
  isCompleted: boolean;
}

export interface QuizQuestion {
  vocabularyId: number;
  questionType: QuestionType;
  word: string;
  meaning?: string;
  partOfSpeech?: string;
  cefrLevel?: string;
  pronunciation?: string;
  imageUrl?: string;
  description?: string;
  options?: string[];
  pronunciationUrl?: string;
}


export interface AnswerRequest {
  vocabularyId: number;
  questionType: QuestionType;
  answer: string;
}

export interface AnswerResponse {
  questionId: number;
  isCorrect: boolean;
}

export interface UpdateProgressResponse{
  vocabId: number;
  proficiencyLevel: number;
}

export interface CompleteLearningSessionResponseDto {
  xpEarned: number;
  isPetRewardGranted: boolean;
  petId?: number;
  petName?: string;
  description?: string;
  imageUrl?: string;
  petRarity?: string;
  petType?: string;
  message: string;
}

export interface CompleteReviewSessionResponseDto{
  xpEarned: number;
  apEarned: number;
  message: string;
}

export const QuestionType = {
  Flashcard: 0,
  FillInBlank: 1,
  MultipleChoice: 2,
  Listening: 3,
} as const;

export interface Pet {
  isOwned: boolean;
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
}

export interface LevelStatDto {
  level: number;
  count: number;
}

export interface UserDashboardDto {
  reviewWordCount: number;
  nextReviewTime: string | null;

  username: string;
  level: number;
  totalXP: number;
  totalAP: number;
  streakDays: number;
  petCount: number;
  avatarUrl?: string;

  vocabularyStats: LevelStatDto[];
}

export interface NotificationDto {
  id: number;
  userId?: number;
  title: string;
  type: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
}

export interface ActivityLogDto {
  id: number;
  userId: number;
  username: string;
  action: string;
  details: string;
  timestamp: Date;
}

export interface AssignRoleDto {
  roleName: string;
}

export type QuestionType = typeof QuestionType[keyof typeof QuestionType];



