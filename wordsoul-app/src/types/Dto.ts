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
  id: number;
  vocabId: number;
  type: string; // multiple-choice, listening, ...
  prompt: string;
  options?: string[];
}

export interface AnswerRequest{
  questionId: number;
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

export interface CompleteSessionResponseDto{
  xpEarned: number;
  isPetRewardGranted: boolean;
  petId? : number;
  message: string;
}