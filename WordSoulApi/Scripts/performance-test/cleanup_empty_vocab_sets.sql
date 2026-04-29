-- ============================================================
--  WordSoul – Xóa liên kết UserVocabularySets trống cho test accounts
--  Xóa các bộ từ vựng id: 1,3,4,5,6,7 (không có từ vựng nào)
--  khỏi 100 test accounts (@wordsoul.test)
-- ============================================================

-- Bước 1: Kiểm tra trước khi xóa
SELECT
    u.Email,
    uvs.VocabularySetId,
    vs.Title
FROM UserVocabularySets uvs
JOIN Users u          ON u.Id = uvs.UserId
JOIN VocabularySets vs ON vs.Id = uvs.VocabularySetId
WHERE u.Email LIKE '%@wordsoul.test'
  AND uvs.VocabularySetId IN (1, 3, 4, 5, 6, 7)
ORDER BY u.Email, uvs.VocabularySetId;

-- Bước 2: Xóa
DELETE uvs
FROM UserVocabularySets uvs
JOIN Users u ON u.Id = uvs.UserId
WHERE u.Email LIKE '%@wordsoul.test'
  AND uvs.VocabularySetId IN (1, 3, 4, 5, 6, 7);

PRINT CONCAT('Đã xóa ', @@ROWCOUNT, ' bản ghi UserVocabularySets trống.');

-- Bước 3: Kiểm tra còn lại
SELECT
    uvs.VocabularySetId,
    vs.Title,
    COUNT(*) AS UserCount
FROM UserVocabularySets uvs
JOIN Users u          ON u.Id = uvs.UserId
JOIN VocabularySets vs ON vs.Id = uvs.VocabularySetId
WHERE u.Email LIKE '%@wordsoul.test'
GROUP BY uvs.VocabularySetId, vs.Title
ORDER BY uvs.VocabularySetId;
