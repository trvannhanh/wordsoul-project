-- ============================================================
--  WordSoul – Seed UserVocabularySets cho 100 test accounts
--  Gán tất cả VocabularySet đang active vào mỗi test account
--  ⚠️ Chỉ chạy trên môi trường DEV/TEST
--  ⚙️ Chạy SAU khi đã chạy seed_test_accounts.sql
-- ============================================================

-- ─── Map với UserVocabularySet entity ──────────────────────
--  UserId                  INT   (FK → Users)
--  VocabularySetId         INT   (FK → VocabularySets)
--  TotalCompletedSession   INT   default 0
--  IsCompleted             BIT   default 0 (false)
--  CreatedAt               DATETIME
--  IsActive                BIT   default 1 (true)
-- ─────────────────────────────────────────────────────────────

-- Bước 1: Kiểm tra trước – bao nhiêu VocabularySet đang active?
SELECT Id, Title, IsActive
FROM VocabularySets
WHERE IsActive = 1;

-- ─────────────────────────────────────────────────────────────
-- Bước 2: Gán toàn bộ VocabularySet active cho mỗi test account
--         (bỏ qua nếu bản ghi đã tồn tại)
-- ─────────────────────────────────────────────────────────────
INSERT INTO UserVocabularySets (
    UserId, VocabularySetId,
    TotalCompletedSession, IsCompleted,
    IsActive, CreatedAt
)
SELECT
    u.Id        AS UserId,
    vs.Id       AS VocabularySetId,
    0           AS TotalCompletedSession,
    0           AS IsCompleted,   -- false: chưa hoàn thành
    1           AS IsActive,      -- true
    GETUTCDATE() AS CreatedAt
FROM Users u
CROSS JOIN VocabularySets vs
WHERE
    u.Email LIKE '%@wordsoul.test'   -- chỉ test accounts
    AND vs.IsActive = 1              -- chỉ set đang active
    -- Bỏ qua nếu đã có bản ghi (tránh duplicate)
    AND NOT EXISTS (
        SELECT 1
        FROM UserVocabularySets uvs
        WHERE uvs.UserId = u.Id
          AND uvs.VocabularySetId = vs.Id
    );

-- ─────────────────────────────────────────────────────────────
-- Bước 3: Kiểm tra kết quả
-- ─────────────────────────────────────────────────────────────
SELECT
    u.Email,
    vs.Title        AS VocabularySetTitle,
    uvs.IsCompleted,
    uvs.IsActive,
    uvs.CreatedAt
FROM Users u
JOIN UserVocabularySets uvs ON uvs.UserId = u.Id
JOIN VocabularySets vs      ON vs.Id = uvs.VocabularySetId
WHERE u.Email LIKE '%@wordsoul.test'
ORDER BY u.Email, vs.Id;

-- Tổng kết
SELECT
    COUNT(DISTINCT UserId)          AS TestAccountsWithSets,
    COUNT(DISTINCT VocabularySetId) AS SetsAssigned,
    COUNT(*)                        AS TotalRecords
FROM UserVocabularySets uvs
JOIN Users u ON u.Id = uvs.UserId
WHERE u.Email LIKE '%@wordsoul.test';
