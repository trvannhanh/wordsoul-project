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
  // ── BASIC (Beginner) ─────────────────────────────────
  DailyLife: 0,     // Normal   - Cuộc sống hàng ngày
  Nature: 1,        // Grass    - Thiên nhiên, cây cỏ, động vật
  Food: 2,          // Fire     - Ẩm thực, nấu nướng (lửa)
  Weather: 3,       // Flying   - Thời tiết, bầu trời

  // ── INTERMEDIATE ─────────────────────────────────────
  Technology: 4,    // Electric - Công nghệ, điện tử
  Travel: 5,        // Ground   - Địa lý, du lịch, di chuyển qua các vùng đất
  Health: 6,        // Fairy    - Sức khỏe, y tế, chữa lành
  Sports: 7,        // Fighting - Thể thao, võ thuật, rèn luyện

  // ── ADVANCED / SPECIALIZED ───────────────────────────
  Business: 8,      // Steel    - Kinh doanh, công nghiệp, tài chính
  Science: 9,       // Psychic  - Khoa học, triết học, tư duy logic
  Art: 10,          // Bug      - Nghệ thuật, sự tỉ mỉ, thủ công
  Communication: 11,// Water    - Giao tiếp, mạng lưới xã hội (dòng chảy thông tin)

  // ── ABSTRACT ─────────────────────────────────────────
  Mystery: 12,      // Ghost    - Tâm linh, bí ẩn, truyền thuyết
  Dark: 13,         // Dark     - Tội phạm, pháp luật, góc tối xã hội
  Academic: 14,     // Ice      - Học thuật khô khan, lý trí, chuyên ngành

  // ── SPECIAL ──────────────────────────────────────────
  Challenge: 15,    // Rock     - Luyện thi, từ vựng khó, thử thách kiên cố
  TrapWords: 16,    // Poison   - Idioms, False friends, từ dễ gây nhầm lẫn (độc)
  System: 17,       // Dragon   - Hệ thống vĩ mô, chính trị, luật lệ cổ đại

  // ── ARTIFICIAL ───────────────────────────────────────
  Custom: 18        // Không hệ / Nhân tạo / Artificial - Ditto, Porygon, Mewtwo
} as const;


export type VocabularySetThemeEnum = typeof VocabularySetThemeEnum[keyof typeof VocabularySetThemeEnum];