export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface RefreshTokenRequestDto {
  id: number;
  refreshToken: string;
}

export interface TokenResponseDto {
  accessToken: string;
  refreshToken: string;
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

export interface LevelStatDto {
  level: number;
  count: number;
}

export interface UserProgressDto {
  reviewWordCount: number;
  nextReviewTime: string | null;
  vocabularyStats: LevelStatDto[];
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

export interface LeaderBoardDto {
  userId: number;
  userName: string;
  totalXP: number;
  totalAP: number;
}