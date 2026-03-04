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
  // ── DAILY / BEGINNER ─────────────────────────────────
  DailyLife: 0,         // Hệ Normal  — từ thường dùng hàng ngày
  Nature: 1,           // Hệ Grass   — động vật, thực vật, môi trường
  Weather: 2,          // Hệ Ice     — thời tiết, mùa, khí hậu
  Food: 3,             // Hệ Water   — ẩm thực, nấu ăn, nhà hàng

  // ── INTERMEDIATE ─────────────────────────────────────
  Technology: 4,       // Hệ Electric — IT, gadget, internet
  Travel: 5,           // Hệ Flying   — du lịch, địa điểm, phương tiện
  Health: 6,           // Hệ Fairy    — sức khỏe, y tế, cảm xúc
  Sports: 7,           // Hệ Fighting — thể thao, rèn luyện

  // ── ADVANCED / SPECIALIZED ───────────────────────────
  Business: 8,         // Hệ Steel    — kinh doanh, tài chính, đàm phán
  Science: 9,          // Hệ Psychic  — khoa học, nghiên cứu, triết học
  Art: 10,              // Hệ Dragon   — nghệ thuật, âm nhạc, văn học
  Mystery: 11,          // Hệ Ghost    — idioms, slang, từ bí ẩn
  Dark: 12,             // Hệ Dark     — từ tiêu cực, tin tức, xung đột

  // ── SPECIAL ──────────────────────────────────────────
  Custom: 13,           // Hệ Fire     — bộ từ tự tạo, cá nhân hóa
  Challenge: 14,        // Hệ Rock     — từ khó, thi cử, luyện thi
  Poison: 15,           // Hệ Poison   — false friends, dễ nhầm lẫn
} as const;


export type VocabularySetThemeEnum = typeof VocabularySetThemeEnum[keyof typeof VocabularySetThemeEnum];