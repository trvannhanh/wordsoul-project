import type { VocabularyDto } from "./VocabularyDto";


export interface VocabularySetDto {
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

export interface VocabularySetDetailDto extends VocabularySetDto {
  vocabularies: VocabularyDto[];
  totalVocabularies: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

export const VocabularySetThemeEnum = {
  DailyLearning: 0,
  AdvancedTopics: 1,
} as const;


export type VocabularySetThemeEnum = typeof VocabularySetThemeEnum[keyof typeof VocabularySetThemeEnum];