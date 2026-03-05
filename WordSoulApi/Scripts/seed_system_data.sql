-- ============================================================
-- WordSoul - System Data Seed (Items, Quests, Achievements)
-- Generated: 2026-03-04 21:36:49
-- HOW TO USE:
--   1. Mở SSMS → Connect Azure SQL
--   2. File → Open → file này
--   3. Đổi [your_database_name] thành tên DB thật
--   4. Execute (F5)
-- ============================================================

USE [your_database_name];
GO

SET IDENTITY_INSERT [Items] ON;
SET IDENTITY_INSERT [DailyQuests] ON;
SET IDENTITY_INSERT [Achievements] ON;

BEGIN TRANSACTION;
BEGIN TRY

-- ================== ITEMS ==================
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (1, N'Soul Coin', N'Đơn vị tiền tệ cơ bản, dùng để mua vật phẩm thông thường.', N'https://cdn-icons-png.flaticon.com/512/3141/3141974.png', 0, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (2, N'Diamond', N'Đơn vị tiền tệ cao cấp, dùng để mua các vật phẩm hiếm.', N'https://cdn-icons-png.flaticon.com/512/3652/3652191.png', 0, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (3, N'Fire Stone', N'Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của ngọn lửa.', N'https://img.pokemondb.net/sprites/items/fire-stone.png', 1, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (4, N'Water Stone', N'Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của dòng nước.', N'https://img.pokemondb.net/sprites/items/water-stone.png', 1, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (5, N'Thunder Stone', N'Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của sấm sét.', N'https://img.pokemondb.net/sprites/items/thunder-stone.png', 1, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (6, N'Leaf Stone', N'Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của cây cỏ.', N'https://img.pokemondb.net/sprites/items/leaf-stone.png', 1, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (7, N'Moon Stone', N'Đá tiến hóa. Một viên đá kì lạ tỏa sáng dịu nhẹ như ánh trăng.', N'https://img.pokemondb.net/sprites/items/moon-stone.png', 1, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (8, N'XP Booster x2', N'Nhân đôi lượng XP nhận được trong 1 giờ.', N'https://cdn-icons-png.flaticon.com/512/1000/1000494.png', 2, GETUTCDATE());
    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES (9, N'AP Booster x2', N'Nhân đôi lượng Action Points nhận được trong 1 giờ.', N'https://cdn-icons-png.flaticon.com/512/1376/1376646.png', 2, GETUTCDATE());

-- =============== DAILY QUESTS ===============
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (1, N'Học từ mới (Nhỏ)', N'Hoàn thành học 10 từ vựng mới trong ngày.', 0, 10, 0, 50, NULL, 1, GETUTCDATE());
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (2, N'Học từ mới (Vừa)', N'Hoàn thành học 30 từ vựng mới trong ngày.', 0, 30, 0, 150, NULL, 1, GETUTCDATE());
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (3, N'Ôn tập mỗi ngày', N'Review lại 20 từ vựng cũ.', 1, 20, 1, 30, NULL, 1, GETUTCDATE());
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (4, N'Chăm chỉ ôn tập', N'Review lại 50 từ vựng cũ.', 1, 50, 1, 100, NULL, 1, GETUTCDATE());
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (5, N'Chính xác tuyệt đối', N'Đạt tỉ lệ chính xác 90% trong 1 phiên học.', 2, 90, 0, 100, NULL, 1, GETUTCDATE());
    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES (6, N'Thu thập Pet', N'Bắt hoặc nhận được 1 Pet bất kỳ hôm nay.', 3, 1, 1, 50, NULL, 1, GETUTCDATE());

-- =============== ACHIEVEMENTS ===============
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (1, N'Người Mới Bắt Đầu', N'Thông thạo 50 từ vựng đầu tiên.', 0, 50, 1, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (2, N'Học Giả Trung Cấp', N'Thông thạo 500 từ vựng.', 0, 500, 2, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (3, N'Bậc Thầy Ngôn Ngữ', N'Thông thạo 2000 từ vựng.', 0, 2000, 2, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (4, N'Khởi Đầu Tốt Đẹp', N'Duy trì học tập liên tục trong 7 ngày.', 1, 7, 1, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (5, N'Kiên Trì Bền Bỉ', N'Duy trì học tập liên tục trong 30 ngày.', 1, 30, 2, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (6, N'Kỷ Luật Thép', N'Duy trì học tập liên tục trong 100 ngày.', 1, 100, 8, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (7, N'Chinh Phục Đầu Tiên', N'Hoàn thành học 1 bộ từ vựng 100%.', 2, 1, 1, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (8, N'Chuyên Gia Đa Lĩnh Vực', N'Hoàn thành học 10 bộ từ vựng 100%.', 2, 10, 9, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (9, N'Người Bắt Thú Tương Lai', N'Thu thập kích hoạt được 5 dạng Pet.', 3, 5, 1, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (10, N'Nhà Sưu Tầm', N'Thu thập kích hoạt được 50 dạng Pet.', 3, 50, 2, GETUTCDATE());
    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES (11, N'Chuyên Gia Thú Cưng', N'Thu thập kích hoạt được 100 dạng Pet.', 3, 100, 7, GETUTCDATE());

    COMMIT TRANSACTION;
    PRINT N'[OK] Seed thanh cong Items, Quests, va Achievements!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'[ERR] Loi: ' + ERROR_MESSAGE();
END CATCH

SET IDENTITY_INSERT [Items] OFF;
SET IDENTITY_INSERT [DailyQuests] OFF;
SET IDENTITY_INSERT [Achievements] OFF;
