-- ============================================================
--  WordSoul – Seed 100 test accounts cho k6 Performance Test
--  Password: "Test@123456"
--  ⚠️ Chỉ chạy trên môi trường DEV/TEST, KHÔNG dùng Production
-- ============================================================

-- ─── Map với User.cs entity ────────────────────────────────
--  Id              INT (auto PK)
--  Username        NVARCHAR(100)?
--  Email           NVARCHAR(100) NOT NULL
--  PasswordHash    NVARCHAR(200) NOT NULL
--  XP              INT  default 0
--  AP              INT  default 0
--  HintBalance     INT  default 5
--  Role            INT  default 0  (UserRole.User=0, Admin=1)
--  CreatedAt       DATETIME default UtcNow
--  IsActive        BIT  default 1
--  RefreshToken    NVARCHAR(200)?  -- nullable, bỏ qua
--  PvpRating       INT  default 1000
--  PvpWins         INT  default 0
--  PvpLosses       INT  default 0
--  AvatarUrl       NVARCHAR(300)?  -- nullable, bỏ qua
-- ─────────────────────────────────────────────────────────────

DECLARE @i INT = 1;
DECLARE @email    NVARCHAR(100);
DECLARE @username NVARCHAR(100);

-- Hash của "Test@123456" tạo bằng Microsoft.AspNetCore.Identity.PasswordHasher<User>
-- (đúng thuật toán AuthService.cs: new PasswordHasher<User>().HashPassword(null!, password))
DECLARE @passwordHash NVARCHAR(200) = 'AQAAAAEAACcQAAAAEDsdPannUBg4mgLq1iiyIKpdxvL0lIK9n0RhP9DTuDtnogcXXCzmLpVxzpoIuYpqXw==';

WHILE @i <= 100
BEGIN
  SET @email    = 'testuser' + CAST(@i AS NVARCHAR) + '@wordsoul.test';
  SET @username = 'testuser' + CAST(@i AS NVARCHAR);

  IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @email)
  BEGIN
    INSERT INTO Users (
      Email, Username, PasswordHash,
      XP, AP, HintBalance,
      Role,
      PvpRating, PvpWins, PvpLosses,
      IsActive, CreatedAt
    )
    VALUES (
      @email, @username, @passwordHash,
      0, 0, 5,   -- XP=0, AP=0, HintBalance=5 (default entity)
      0,          -- Role: UserRole.User=0 (EF Core lưu enum dạng int)
      1000, 0, 0, -- PvpRating=1000, PvpWins=0, PvpLosses=0
      1,          -- IsActive = true
      GETUTCDATE()
    );
  END

  SET @i = @i + 1;
END

PRINT 'Seeded 100 test accounts (testuser1@wordsoul.test ... testuser100@wordsoul.test)';

-- ============================================================
--  Kiểm tra
-- ============================================================
SELECT COUNT(*) AS TotalTestAccounts
FROM Users
WHERE Email LIKE '%@wordsoul.test';
