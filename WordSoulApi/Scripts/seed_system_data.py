"""
WordSoul - System Data Seed Script (Items, Quests, Achievements)
================================================================
Generates seed_system_data.sql for basic game system data.

Usage:
    cd Scripts
    python seed_system_data.py

Output:
    seed_system_data.sql
"""

from datetime import datetime

OUTPUT_FILE = "seed_system_data.sql"

# ─────────────────────────────────────────────────────────────────────────────
# ENUM VALUES (must match C# enums)
# ─────────────────────────────────────────────────────────────────────────────
# ItemType        : Currency=0, Evolution=1, Booster=2
# QuestType       : Learn=0, Review=1, Accuracy=2, Catch=3
# RewardType      : XP=0, AP=1, Item=2, Pet=3
# ConditionType   : MasterWords=0, DailyStreak=1, CompletedSet=2, CatchedPets=3

# ─────────────────────────────────────────────────────────────────────────────
# DATA DEFINITIONS
# ─────────────────────────────────────────────────────────────────────────────

ITEMS = [
    # (Id, Name, Description, ImageUrl, TypeInt)
    (1, "Soul Coin", "Đơn vị tiền tệ cơ bản, dùng để mua vật phẩm thông thường.", "https://cdn-icons-png.flaticon.com/512/3141/3141974.png", 0),
    (2, "Diamond", "Đơn vị tiền tệ cao cấp, dùng để mua các vật phẩm hiếm.", "https://cdn-icons-png.flaticon.com/512/3652/3652191.png", 0),
    (3, "Fire Stone", "Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của ngọn lửa.", "https://img.pokemondb.net/sprites/items/fire-stone.png", 1),
    (4, "Water Stone", "Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của dòng nước.", "https://img.pokemondb.net/sprites/items/water-stone.png", 1),
    (5, "Thunder Stone", "Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của sấm sét.", "https://img.pokemondb.net/sprites/items/thunder-stone.png", 1),
    (6, "Leaf Stone", "Đá tiến hóa. Một viên đá kì lạ chứa sức mạnh của cây cỏ.", "https://img.pokemondb.net/sprites/items/leaf-stone.png", 1),
    (7, "Moon Stone", "Đá tiến hóa. Một viên đá kì lạ tỏa sáng dịu nhẹ như ánh trăng.", "https://img.pokemondb.net/sprites/items/moon-stone.png", 1),
    (8, "XP Booster x2", "Nhân đôi lượng XP nhận được trong 1 giờ.", "https://cdn-icons-png.flaticon.com/512/1000/1000494.png", 2),
    (9, "AP Booster x2", "Nhân đôi lượng Action Points nhận được trong 1 giờ.", "https://cdn-icons-png.flaticon.com/512/1376/1376646.png", 2),
]

DAILY_QUESTS = [
    # (Id, Title, Description, QuestType, TargetValue, RewardType, RewardValue, RewardReferenceId)
    # Learn (0)
    (1, "Học từ mới (Nhỏ)", "Hoàn thành học 10 từ vựng mới trong ngày.", 0, 10, 0, 50, None), # Reward 50 XP
    (2, "Học từ mới (Vừa)", "Hoàn thành học 30 từ vựng mới trong ngày.", 0, 30, 0, 150, None), # Reward 150 XP
    # Review (1)
    (3, "Ôn tập mỗi ngày", "Review lại 20 từ vựng cũ.", 1, 20, 1, 30, None), # Reward 30 AP
    (4, "Chăm chỉ ôn tập", "Review lại 50 từ vựng cũ.", 1, 50, 1, 100, None), # Reward 100 AP
    # Accuracy (2)
    (5, "Chính xác tuyệt đối", "Đạt tỉ lệ chính xác 90% trong 1 phiên học.", 2, 90, 0, 100, None),
    # Catch (3)
    (6, "Thu thập Pet", "Bắt hoặc nhận được 1 Pet bất kỳ hôm nay.", 3, 1, 1, 50, None),
]

ACHIEVEMENTS = [
    # (Id, Name, Description, ConditionType, ConditionValue, RewardItemId)
    # MasterWords (0)
    (1, "Người Mới Bắt Đầu", "Thông thạo 50 từ vựng đầu tiên.", 0, 50, 1), # Reward: Soul Coin (id 1)
    (2, "Học Giả Trung Cấp", "Thông thạo 500 từ vựng.", 0, 500, 2), # Reward: Diamond (id 2)
    (3, "Bậc Thầy Ngôn Ngữ", "Thông thạo 2000 từ vựng.", 0, 2000, 2), # Reward: Diamond (id 2)
    
    # DailyStreak (1)
    (4, "Khởi Đầu Tốt Đẹp", "Duy trì học tập liên tục trong 7 ngày.", 1, 7, 1), # Reward: Soul Coin
    (5, "Kiên Trì Bền Bỉ", "Duy trì học tập liên tục trong 30 ngày.", 1, 30, 2), # Reward: Diamond
    (6, "Kỷ Luật Thép", "Duy trì học tập liên tục trong 100 ngày.", 1, 100, 8), # Reward: XP Booster
    
    # CompletedSet (2)
    (7, "Chinh Phục Đầu Tiên", "Hoàn thành học 1 bộ từ vựng 100%.", 2, 1, 1),
    (8, "Chuyên Gia Đa Lĩnh Vực", "Hoàn thành học 10 bộ từ vựng 100%.", 2, 10, 9), # Reward: AP Booster
    
    # CatchedPets (3)
    (9, "Người Bắt Thú Tương Lai", "Thu thập kích hoạt được 5 dạng Pet.", 3, 5, 1),
    (10, "Nhà Sưu Tầm", "Thu thập kích hoạt được 50 dạng Pet.", 3, 50, 2),
    (11, "Chuyên Gia Thú Cưng", "Thu thập kích hoạt được 100 dạng Pet.", 3, 100, 7), # Reward: Moon Stone (id 7)
]

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────

def q(val):
    if val is None:
        return "NULL"
    return "N'" + str(val).replace("'", "''") + "'"

def v(val):
    return str(val) if val is not None else "NULL"

# ─────────────────────────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print(" WordSoul - System Data Seed (Items, Quests, Achievements)")
    print("=" * 60)

    lines = []

    # 1. Items
    lines.append("-- ================== ITEMS ==================")
    for item in ITEMS:
        id_, name, desc, img, type_int = item
        line = f"    INSERT INTO [Items] ([Id],[Name],[Description],[ImageUrl],[Type],[CreatedDate]) VALUES ({id_}, {q(name)}, {q(desc)}, {q(img)}, {type_int}, GETUTCDATE());"
        lines.append(line)

    lines.append("")
    # 2. DailyQuests
    lines.append("-- =============== DAILY QUESTS ===============")
    for q_item in DAILY_QUESTS:
        id_, title, desc, q_type, target, r_type, r_val, r_ref = q_item
        line = f"    INSERT INTO [DailyQuests] ([Id],[Title],[Description],[QuestType],[TargetValue],[RewardType],[RewardValue],[RewardReferenceId],[IsActive],[CreatedAt]) VALUES " \
               f"({id_}, {q(title)}, {q(desc)}, {q_type}, {target}, {r_type}, {r_val}, {v(r_ref)}, 1, GETUTCDATE());"
        lines.append(line)

    lines.append("")
    # 3. Achievements
    lines.append("-- =============== ACHIEVEMENTS ===============")
    for ach in ACHIEVEMENTS:
        id_, name, desc, c_type, c_val, r_item_id = ach
        line = f"    INSERT INTO [Achievements] ([Id],[Name],[Description],[ConditionType],[ConditionValue],[RewardItemId],[CreatedAt]) VALUES " \
               f"({id_}, {q(name)}, {q(desc)}, {c_type}, {c_val}, {r_item_id}, GETUTCDATE());"
        lines.append(line)

    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("-- ============================================================\n")
        f.write("-- WordSoul - System Data Seed (Items, Quests, Achievements)\n")
        f.write(f"-- Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write("-- HOW TO USE:\n")
        f.write("--   1. Mở SSMS → Connect Azure SQL\n")
        f.write("--   2. File → Open → file này\n")
        f.write("--   3. Đổi [your_database_name] thành tên DB thật\n")
        f.write("--   4. Execute (F5)\n")
        f.write("-- ============================================================\n\n")
        f.write("USE [your_database_name];\n")
        f.write("GO\n\n")

        # Set identity insert on for all three tables
        f.write("SET IDENTITY_INSERT [Items] ON;\n")
        f.write("SET IDENTITY_INSERT [DailyQuests] ON;\n")
        f.write("SET IDENTITY_INSERT [Achievements] ON;\n\n")

        f.write("BEGIN TRANSACTION;\n")
        f.write("BEGIN TRY\n\n")

        f.write("\n".join(lines))

        f.write("\n\n    COMMIT TRANSACTION;\n")
        f.write(f"    PRINT N'[OK] Seed thanh cong Items, Quests, va Achievements!';\n")
        f.write("END TRY\n")
        f.write("BEGIN CATCH\n")
        f.write("    ROLLBACK TRANSACTION;\n")
        f.write("    PRINT N'[ERR] Loi: ' + ERROR_MESSAGE();\n")
        f.write("END CATCH\n\n")

        f.write("SET IDENTITY_INSERT [Items] OFF;\n")
        f.write("SET IDENTITY_INSERT [DailyQuests] OFF;\n")
        f.write("SET IDENTITY_INSERT [Achievements] OFF;\n")

    print(f" [OK] Xong! File '{OUTPUT_FILE}' da duoc tao.")
    print("=" * 60)

if __name__ == "__main__":
    main()
