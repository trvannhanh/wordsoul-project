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
  level: number;
  totalXP: number;
  totalAP: number;
  streakDays: number;
  avatarUrl?: string;
  petCount?: number;
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
  theme: string; // Enum: 'DailyLearning' | 'AdvancedTopics' | ...
  difficultyLevel: string; // Enum: 'Beginner' | 'Intermediate' | 'Advanced' | ...
  imageUrl?: string;
  isActive: boolean;
  createdAt: string; // ISO date string (e.g., '2025-09-13T12:00:00Z')
  isPublic: boolean; // Mới: Trạng thái công khai
  isOwned: boolean; // Mới: Người dùng sở hữu
  createdById?: number; // Mới: ID người tạo
  createdByUsername?: string; // Mới: Username người tạo
  vocabularyIds?: number[]; // Mới: Danh sách ID từ vựng
}

export interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  partOfSpeech: string;
  example: string | null;
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
  petId: number;
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
  isRetry?: boolean; // Thêm thuộc tính isRetry
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

export const VocabularySetTheme = {
  DailyLearning: 0,
  AdvancedTopics: 1,
} as const;

export const VocabularyDifficultyLevel = {
  Beginner: 0,
  Intermediate: 1,
  Advanced: 2
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

export interface PetDetail {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
  level: number | null;
  experience: number | null;
  isFavorite: boolean | null;
  isActive: boolean;
  acquiredAt: string | null;
  baseFormId: number | null;
  nextEvolutionId: number | null;
  requiredLevel: number | null;
}

export interface UpgradePetResponse {
  petId: number;
  experience: number;
  level: number;
  isLevelUp: boolean;
  isEvolved: boolean;
  ap: number;
}



export interface LevelStatDto {
  level: number;
  count: number;
}

export interface UserProgressDto {
  reviewWordCount: number;
  nextReviewTime: string | null;
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

export interface UserVocabularySetDto {
  vocabularySetId: number;
  totalCompletedSessions: number;
  isCompleted: boolean;
  createdAt: string;
  isActive: boolean;
}

export interface SearchVocabularyDto {
  words: string[];
}


export type QuestionType = typeof QuestionType[keyof typeof QuestionType];


export type VocabularySetTheme = typeof VocabularySetTheme[keyof typeof VocabularySetTheme];

export type VocabularyDifficultyLevel = typeof VocabularyDifficultyLevel[keyof typeof VocabularyDifficultyLevel];




