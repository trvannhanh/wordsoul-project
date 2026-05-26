import type { VocabularyDto } from './VocabularyDto';

export interface VocabularySetDto {
  id: number;
  title: string;
  description: string | null;
  theme: string;
  difficultyLevel: string;
  imageUrl?: string;
  isActive: boolean;
  createdAt: string;
  isPublic: boolean;
  isOwned: boolean;
  createdById?: number;
  createdByUsername?: string;
  vocabularyIds?: number[];
}

export interface VocabularySetDetailDto extends VocabularySetDto {
  vocabularies: VocabularyDto[];
  totalVocabularies: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

export interface VocabularySetProgressDto {
  vocabularySetId: number;
  totalVocabularies: number;
  learnedVocabularies: number;
  progressPercent: number;
  lastStudiedAt?: string;
}

export const VocabularySetThemeEnum = {
  DailyLife: 0,
  Nature: 1,
  Food: 2,
  Weather: 3,
  Technology: 4,
  Travel: 5,
  Health: 6,
  Sports: 7,
  Business: 8,
  Science: 9,
  Art: 10,
  Communication: 11,
  Mystery: 12,
  Dark: 13,
  Academic: 14,
  Challenge: 15,
  TrapWords: 16,
  System: 17,
  Custom: 18,
} as const;

export type VocabularySetThemeEnum =
  (typeof VocabularySetThemeEnum)[keyof typeof VocabularySetThemeEnum];

// Tên hiển thị cho từng theme
export const VocabularySetThemeLabel: Record<string, string> = {
  DailyLife: 'Cuộc sống hàng ngày',
  Nature: 'Thiên nhiên',
  Food: 'Ẩm thực',
  Weather: 'Thời tiết',
  Technology: 'Công nghệ',
  Travel: 'Du lịch',
  Health: 'Sức khỏe',
  Sports: 'Thể thao',
  Business: 'Kinh doanh',
  Science: 'Khoa học',
  Art: 'Nghệ thuật',
  Communication: 'Giao tiếp',
  Mystery: 'Bí ẩn',
  Dark: 'Pháp luật',
  Academic: 'Học thuật',
  Challenge: 'Thử thách',
  TrapWords: 'Idioms & Bẫy',
  System: 'Hệ thống',
  Custom: 'Tùy chỉnh',
};
