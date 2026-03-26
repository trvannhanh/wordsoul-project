

namespace WordSoul.Domain.Enums
{
    public enum VocabularySetTheme
    {
        // ── BASIC (Beginner) ─────────────────────────────────
        DailyLife,      // Normal   - Cuộc sống hàng ngày
        Nature,         // Grass    - Thiên nhiên, cây cỏ, động vật
        Food,           // Fire     - Ẩm thực, nấu nướng (lửa)
        Weather,        // Flying   - Thời tiết, bầu trời

        // ── INTERMEDIATE ─────────────────────────────────────
        Technology,     // Electric - Công nghệ, điện tử
        Travel,         // Ground   - Địa lý, du lịch, di chuyển qua các vùng đất
        Health,         // Fairy    - Sức khỏe, y tế, chữa lành
        Sports,         // Fighting - Thể thao, võ thuật, rèn luyện

        // ── ADVANCED / SPECIALIZED ───────────────────────────
        Business,       // Steel    - Kinh doanh, công nghiệp, tài chính
        Science,        // Psychic  - Khoa học, triết học, tư duy logic
        Art,            // Bug      - Nghệ thuật, sự tỉ mỉ, thủ công
        Communication,  // Water    - Giao tiếp, mạng lưới xã hội (dòng chảy thông tin)
        Social,         // Xóa do trùng Water -> Hoặc có thể hiểu Communication = Water. (Sẽ dùng Communication)

        // ── ABSTRACT ─────────────────────────────────────────
        Mystery,        // Ghost    - Tâm linh, bí ẩn, truyền thuyết
        Dark,           // Dark     - Tội phạm, pháp luật, góc tối xã hội
        Academic,       // Ice      - Học thuật khô khan, lý trí, chuyên ngành

        // ── SPECIAL ──────────────────────────────────────────
        Challenge,      // Rock     - Luyện thi, từ vựng khó, thử thách kiên cố
        TrapWords,      // Poison   - Idioms, False friends, từ dễ gây nhầm lẫn (độc)
        System,         // Dragon   - Hệ thống vĩ mô, chính trị, luật lệ cổ đại
        
        // ── ARTIFICIAL ───────────────────────────────────────
        Custom          // (Không hệ / Nhân tạo / Artificial) - Bộ từ do User tự tạo hoặc liên quan đến Ditto, Porygon, Mewtwo...
    }
}
