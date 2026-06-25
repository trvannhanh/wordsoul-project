-- ============================================================
-- Pronunciation Practice — Seed Data (Phase 3)
-- Chạy script này 1 lần sau khi đã apply migration AddPronunciationPractice
-- Sử dụng IF NOT EXISTS để idempotent (có thể chạy lại an toàn)
-- ============================================================

-- ─────────────────────────────────────────────────────────────
-- 1. DAILY QUEST TEMPLATES — QuestType = 4 (Pronunciation)
-- RewardType: 0 = XP, 1 = AP, 2 = Item
-- Thêm 3 mức độ khó để phù hợp nhiều level người dùng
-- ─────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM DailyQuests
    WHERE Title = N'Tập Nói - Cơ Bản'
      AND QuestType = 4
)
BEGIN
    INSERT INTO DailyQuests (Title, Description, QuestType, TargetValue, RewardType, RewardValue, RewardReferenceId, IsActive, CreatedAt)
    VALUES (
        N'Tập Nói - Cơ Bản',
        N'Phát âm chuẩn 3 từ trong ngày',
        4,     -- QuestType.Pronunciation
        3,     -- TargetValue: 3 từ
        0,     -- RewardType.XP
        30,    -- 30 XP
        NULL,
        1,
        GETUTCDATE()
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM DailyQuests
    WHERE Title = N'Tập Nói - Tiêu Chuẩn'
      AND QuestType = 4
)
BEGIN
    INSERT INTO DailyQuests (Title, Description, QuestType, TargetValue, RewardType, RewardValue, RewardReferenceId, IsActive, CreatedAt)
    VALUES (
        N'Tập Nói - Tiêu Chuẩn',
        N'Phát âm chuẩn 7 từ trong ngày',
        4,     -- QuestType.Pronunciation
        7,     -- TargetValue: 7 từ
        0,     -- RewardType.XP
        80,    -- 80 XP
        NULL,
        1,
        GETUTCDATE()
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM DailyQuests
    WHERE Title = N'Tập Nói - Nâng Cao'
      AND QuestType = 4
)
BEGIN
    INSERT INTO DailyQuests (Title, Description, QuestType, TargetValue, RewardType, RewardValue, RewardReferenceId, IsActive, CreatedAt)
    VALUES (
        N'Tập Nói - Nâng Cao',
        N'Phát âm chuẩn 15 từ trong ngày — Thử thách hàng ngày!',
        4,     -- QuestType.Pronunciation
        15,    -- TargetValue: 15 từ
        0,     -- RewardType.XP
        200,   -- 200 XP
        NULL,
        1,
        GETUTCDATE()
    );
END;

-- ─────────────────────────────────────────────────────────────
-- 2. ACHIEVEMENTS — ConditionType = 5 (PronunciationMastered)
-- Milestone progression: 10 → 50 → 100 → 250 → 500 từ Perfect
-- ─────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM Achievements
    WHERE Name = N'Người Học Nói'
      AND ConditionType = 5
)
BEGIN
    INSERT INTO Achievements (Name, Description, ConditionType, ConditionValue, RewardItemId, RewardXp)
    VALUES (
        N'Người Học Nói',
        N'Phát âm chuẩn 10 từ tổng cộng',
        5,     -- ConditionType.PronunciationMastered
        10,    -- 10 từ Perfect
        NULL,
        50     -- RewardXp
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM Achievements
    WHERE Name = N'Lưỡi Vàng'
      AND ConditionType = 5
)
BEGIN
    INSERT INTO Achievements (Name, Description, ConditionType, ConditionValue, RewardItemId, RewardXp)
    VALUES (
        N'Lưỡi Vàng',
        N'Phát âm chuẩn 50 từ tổng cộng',
        5,
        50,
        NULL,
        150
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM Achievements
    WHERE Name = N'Bậc Thầy Phát Âm'
      AND ConditionType = 5
)
BEGIN
    INSERT INTO Achievements (Name, Description, ConditionType, ConditionValue, RewardItemId, RewardXp)
    VALUES (
        N'Bậc Thầy Phát Âm',
        N'Phát âm chuẩn 100 từ tổng cộng — Thực sự thành thạo!',
        5,
        100,
        NULL,
        300
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM Achievements
    WHERE Name = N'Diễn Giả Tinh Tế'
      AND ConditionType = 5
)
BEGIN
    INSERT INTO Achievements (Name, Description, ConditionType, ConditionValue, RewardItemId, RewardXp)
    VALUES (
        N'Diễn Giả Tinh Tế',
        N'Phát âm chuẩn 250 từ tổng cộng',
        5,
        250,
        NULL,
        600
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM Achievements
    WHERE Name = N'Huyền Thoại Ngôn Ngữ'
      AND ConditionType = 5
)
BEGIN
    INSERT INTO Achievements (Name, Description, ConditionType, ConditionValue, RewardItemId, RewardXp)
    VALUES (
        N'Huyền Thoại Ngôn Ngữ',
        N'Phát âm chuẩn 500 từ tổng cộng — Bậc thần ngôn ngữ!',
        5,
        500,
        NULL,
        1500
    );
END;

-- ─────────────────────────────────────────────────────────────
-- 3. KHỞI TẠO UserAchievement cho các Achievement mới
--    (cho tất cả user hiện tại đã có UserAchievement records)
--    Script này chỉ thêm record còn thiếu, không ảnh hưởng tiến độ cũ
-- ─────────────────────────────────────────────────────────────

INSERT INTO UserAchievements (UserId, AchievementId, ProgressValue, IsCompleted, IsClaimed)
SELECT
    u.Id AS UserId,
    a.Id AS AchievementId,
    0     AS ProgressValue,
    0     AS IsCompleted,
    0     AS IsClaimed
FROM Users u
CROSS JOIN Achievements a
WHERE a.ConditionType = 5  -- Chỉ PronunciationMastered achievements
  AND NOT EXISTS (
      SELECT 1 FROM UserAchievements ua
      WHERE ua.UserId = u.Id AND ua.AchievementId = a.Id
  );

-- ─────────────────────────────────────────────────────────────
-- Verify
-- ─────────────────────────────────────────────────────────────

SELECT 'DailyQuests (Pronunciation)' AS [Table], COUNT(*) AS [Count]
FROM DailyQuests WHERE QuestType = 4

UNION ALL

SELECT 'Achievements (PronunciationMastered)', COUNT(*)
FROM Achievements WHERE ConditionType = 5

UNION ALL

SELECT 'UserAchievements (Pronunciation)', COUNT(*)
FROM UserAchievements ua
JOIN Achievements a ON ua.AchievementId = a.Id
WHERE a.ConditionType = 5;
