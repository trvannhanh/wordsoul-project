using Microsoft.EntityFrameworkCore;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence
{
    public class WordSoulDbContext(DbContextOptions<WordSoulDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VocabularySet> VocabularySets { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<AnswerRecord> AnswerRecords { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<LearningSession> LearningSessions { get; set; }
        public DbSet<UserVocabularySet> UserVocabularySets { get; set; }
        public DbSet<SetVocabulary> SetVocabularies { get; set; }
        public DbSet<SessionVocabulary> SessionVocabularies { get; set; }
        public DbSet<UserVocabularyProgress> UserVocabularyProgresses { get; set; }
        public DbSet<UserOwnedPet> UserOwnedPets { get; set; }
        public DbSet<SetRewardPet> SetRewardPets { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<UserItem> UserItems { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<VocabularyReviewHistory> VocabularyReviewHistories { get; set; }
        public DbSet<DailyQuest> DailyQuests { get; set; }
        public DbSet<UserDailyQuest> UserDailyQuests { get; set; }

        // ── Gym Leader Progression System ────────────────
        public DbSet<GymLeader> GymLeaders { get; set; }
        public DbSet<UserGymProgress> UserGymProgresses { get; set; }
        public DbSet<BattleSession> BattleSessions { get; set; }
        public DbSet<BattleAnswer> BattleAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can go here

            modelBuilder.Entity<ActivityLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);  // Không xóa user nếu có log

            // Đảm bảo unique constraint trên ( LearningSessionId, QuizQuestionId, QuestionType)
            modelBuilder.Entity<AnswerRecord>()
                .HasIndex(ar => new { ar.LearningSessionId, ar.VocabularyId, ar.QuestionType });


            //Vocabulary 1 - N AnserRecord relationship
            modelBuilder.Entity<Vocabulary>()
                .HasMany(v => v.AnswerRecords)
                .WithOne(q => q.Vocabulary)
                .HasForeignKey(q => q.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if vocabulary is deleted

            //LearningSession 1 - N AnswerRecord relationship
            modelBuilder.Entity<LearningSession>()
                .HasMany(ls => ls.AnswerRecords)
                .WithOne(a => a.LearningSession)
                .HasForeignKey(a => a.LearningSessionId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to prevent accidental loss of answer records


            // User 1 - N LearningSession relationship
            modelBuilder.Entity<User>() 
                .HasMany(u => u.LearningSessions)
                .WithOne(ls => ls.User) 
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            // User 1 - N Notification relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(ls => ls.User)
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            // VocabularySet 1 - N LearningSession relationship
            modelBuilder.Entity<VocabularySet>()
                .HasMany(vs => vs.LearningSessions)
                .WithOne(ls => ls.VocabularySet)
                .HasForeignKey(ls => ls.VocabularySetId)  
                .OnDelete(DeleteBehavior.NoAction); // No cascade delete, as we want to keep sessions even if the set is deleted

            // Cấu hình mối quan hệ CreatedBy (1-N: User -> VocabularySet)
            modelBuilder.Entity<VocabularySet>()
                .HasOne(vs => vs.CreatedBy)
                .WithMany(u => u.CreatedVocabularySets)  // Nếu bạn thêm collection ở User
                .HasForeignKey(vs => vs.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);  // Tránh xóa cascade nếu user bị xóa

            // User N - N VocabularySet relationship (UserVocabularySet)
            modelBuilder.Entity<UserVocabularySet>()
            .HasKey(uvs => new { uvs.UserId, uvs.VocabularySetId }); // Khóa chính composite

            modelBuilder.Entity<UserVocabularySet>()
                .HasOne(uvs => uvs.User)
                .WithMany(u => u.UserVocabularySets)
                .HasForeignKey(uvs => uvs.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            modelBuilder.Entity<UserVocabularySet>()
                .HasOne(uvs => uvs.VocabularySet)
                .WithMany(p => p.UserVocabularySets)
                .HasForeignKey(uvs => uvs.VocabularySetId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if vocabulary set is deleted

            // Cấu hình UserOwnedPet
            modelBuilder.Entity<UserOwnedPet>()
                .HasKey(uop => uop.Id); // Đặt Id là khóa chính

            modelBuilder.Entity<UserOwnedPet>()
                .Property(uop => uop.Id)
                .ValueGeneratedOnAdd(); // Tự động tăng

            modelBuilder.Entity<UserOwnedPet>()
                .HasOne(uop => uop.User)
                .WithMany(u => u.UserOwnedPets)
                .HasForeignKey(uop => uop.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa UserOwnedPet

            modelBuilder.Entity<UserOwnedPet>()
                .HasOne(uop => uop.Pet)
                .WithMany(p => p.UserOwnedPets)
                .HasForeignKey(uop => uop.PetId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa Pet nếu UserOwnedPet bị xóa

            // Pet N - N VocabularySet relationship (SetRewardPet)
            modelBuilder.Entity<SetRewardPet>()
            .HasKey(ps => new { ps.PetId, ps.VocabularySetId }); // Khóa chính composite

            modelBuilder.Entity<SetRewardPet>()
                .HasOne(srp => srp.Pet)
                .WithMany(p => p.SetRewardPets)
                .HasForeignKey(srp => srp.PetId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if pet is deleted

            modelBuilder.Entity<SetRewardPet>()
                .HasOne(srp => srp.VocabularySet)
                .WithMany(vs => vs.SetRewardPets)
                .HasForeignKey(srp => srp.VocabularySetId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if vocabulary set is deleted

            // User N - N Item relationship (UserItem)

            modelBuilder.Entity<UserItem>()
               .HasKey(ui => ui.Id); // Đặt Id là khóa chính


            modelBuilder.Entity<UserItem>()
                .Property(ui => ui.Id)
                .ValueGeneratedOnAdd(); // Tự động tăng

            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.Item)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.ItemId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if pet is deleted

            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserItems)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if pet is deleted

            // User N - N Achievement relationship (UserItem)

            modelBuilder.Entity<UserAchievement>()
              .HasKey(ua => ua.Id); // Đặt Id là khóa chính

            modelBuilder.Entity<UserAchievement>()
                .Property(ua => ua.Id)
                .ValueGeneratedOnAdd(); // Tự động tăng


            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if pet is deleted

            modelBuilder.Entity<UserAchievement>()
               .HasOne(ua => ua.User)
               .WithMany(u => u.UserAchievements)
               .HasForeignKey(ua => ua.UserId)
               .OnDelete(DeleteBehavior.Cascade); // Cascade delete if pet is deleted


            // User N - N Vocabulary relationship (UserVocabularyProgress)
            modelBuilder.Entity<UserVocabularyProgress>()
            .HasKey(uvp => new { uvp.UserId, uvp.VocabularyId }); // Khóa chính composite

            modelBuilder.Entity<UserVocabularyProgress>()
                .HasOne(uvp => uvp.User)
                .WithMany(u => u.UserVocabularyProgresses)
                .HasForeignKey(uvp => uvp.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            modelBuilder.Entity<UserVocabularyProgress>()
                .HasOne(uvp => uvp.Vocabulary)
                .WithMany(v => v.UserVocabularyProgresses)
                .HasForeignKey(uvp => uvp.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete if vocabulary is deleted, to prevent accidental loss of progress

            // VocabularySet N - N Vocabulary relationship (SetVocabulary)
            modelBuilder.Entity<SetVocabulary>()
                .HasKey(vsv => new { vsv.VocabularySetId, vsv.VocabularyId }); // Khóa chính composite

            modelBuilder.Entity<SetVocabulary>()
                .HasOne(sv => sv.VocabularySet)
                .WithMany(vs => vs.SetVocabularies)
                .HasForeignKey(sv => sv.VocabularySetId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if vocabulary set is deleted

            modelBuilder.Entity<SetVocabulary>()
                .HasOne(sv => sv.Vocabulary)
                .WithMany(v => v.SetVocabularies)
                .HasForeignKey(sv => sv.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if vocabulary is deleted

            // LearningSession N - N Vocabulary relationship (SessionVocabulary)
            modelBuilder.Entity<SessionVocabulary>()
                .HasKey(lsv => new { lsv.LearningSessionId, lsv.VocabularyId }); // Khóa chính composite

            modelBuilder.Entity<SessionVocabulary>()
                .HasOne(sv => sv.LearningSession)
                .WithMany(ls => ls.SessionVocabularies)
                .HasForeignKey(sv => sv.LearningSessionId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if learning session is deleted

            modelBuilder.Entity<SessionVocabulary>()
                .HasOne(sv => sv.Vocabulary)
                .WithMany(v => v.SessionVocabularies)
                .HasForeignKey(sv => sv.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete if vocabulary is deleted, to prevent accidental loss of session vocabularies

            modelBuilder.Entity<UserVocabularyProgress>()
                .Property(x => x.RetentionScore)
                .HasPrecision(5, 2);

            // Configure VocabularyReviewHistory relationships
            modelBuilder.Entity<VocabularyReviewHistory>()
                .HasOne(vrh => vrh.User)
                .WithMany()
                .HasForeignKey(vrh => vrh.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            modelBuilder.Entity<VocabularyReviewHistory>()
                .HasOne(vrh => vrh.Vocabulary)
                .WithMany()
                .HasForeignKey(vrh => vrh.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of vocabulary

            // Indexes for ActivityLog to optimize common queries
            modelBuilder.Entity<ActivityLog>()
                .HasIndex(al => al.UserId);
            modelBuilder.Entity<ActivityLog>()
                .HasIndex(al => al.Timestamp);
            modelBuilder.Entity<ActivityLog>()
                .HasIndex(al => al.Action);

            // Indexes for Vocabulary to optimize search and filtering
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => v.Word);
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => new { v.PartOfSpeech, v.CEFRLevel });

            // Indexes for VocabularySet to optimize search and filtering
            modelBuilder.Entity<VocabularySet>()
                .HasIndex(vs => vs.Title);
            modelBuilder.Entity<VocabularySet>()
                .HasIndex(vs => new { vs.Theme, vs.DifficultyLevel, vs.CreatedAt });
            modelBuilder.Entity<VocabularySet>()
                .HasIndex(vs => vs.IsPublic);

            // Indexes for SetVocabulary to optimize 
            modelBuilder.Entity<SetVocabulary>()
                .HasIndex(sv => sv.VocabularySetId);
            modelBuilder.Entity<SetVocabulary>()
                .HasIndex(sv => sv.Order);

            // Configure DailyQuest
            modelBuilder.Entity<DailyQuest>()
                .Property(dq => dq.Title)
                .HasMaxLength(100);

            modelBuilder.Entity<DailyQuest>()
                .Property(dq => dq.Description)
                .HasMaxLength(300);

            // Configure UserDailyQuest relationships
            modelBuilder.Entity<UserDailyQuest>()
                .HasOne(udq => udq.User)
                .WithMany(u => u.UserDailyQuests)
                .HasForeignKey(udq => udq.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDailyQuest>()
                .HasOne(udq => udq.DailyQuest)
                .WithMany()
                .HasForeignKey(udq => udq.DailyQuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDailyQuest>()
                .HasIndex(udq => new { udq.UserId, udq.DailyQuestId, udq.QuestDate })
                .IsUnique();

            // ── Gym Leader Progression ────────────────────────────────────────────

            // UserGymProgress: composite PK
            modelBuilder.Entity<UserGymProgress>()
                .HasKey(ugp => new { ugp.UserId, ugp.GymLeaderId });

            modelBuilder.Entity<UserGymProgress>()
                .HasOne(ugp => ugp.User)
                .WithMany(u => u.UserGymProgresses)
                .HasForeignKey(ugp => ugp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGymProgress>()
                .HasOne(ugp => ugp.GymLeader)
                .WithMany(gl => gl.UserGymProgresses)
                .HasForeignKey(ugp => ugp.GymLeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // GymLeader → Achievement (badge)
            modelBuilder.Entity<GymLeader>()
                .HasOne(gl => gl.BadgeAchievement)
                .WithMany()
                .HasForeignKey(gl => gl.BadgeAchievementId)
                .OnDelete(DeleteBehavior.SetNull);

            // BattleSession: Challenger
            modelBuilder.Entity<BattleSession>()
                .HasOne(bs => bs.ChallengerUser)
                .WithMany()
                .HasForeignKey(bs => bs.ChallengerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // BattleSession: Opponent (nullable)
            modelBuilder.Entity<BattleSession>()
                .HasOne(bs => bs.OpponentUser)
                .WithMany()
                .HasForeignKey(bs => bs.OpponentUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // BattleSession: GymLeader (nullable)
            modelBuilder.Entity<BattleSession>()
                .HasOne(bs => bs.GymLeader)
                .WithMany(gl => gl.BattleSessions)
                .HasForeignKey(bs => bs.GymLeaderId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // BattleAnswer
            modelBuilder.Entity<BattleAnswer>()
                .HasOne(ba => ba.BattleSession)
                .WithMany(bs => bs.Answers)
                .HasForeignKey(ba => ba.BattleSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BattleAnswer>()
                .HasOne(ba => ba.Vocabulary)
                .WithMany()
                .HasForeignKey(ba => ba.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            modelBuilder.Entity<BattleSession>()
                .HasIndex(bs => new { bs.ChallengerUserId, bs.Status });
            modelBuilder.Entity<UserGymProgress>()
                .HasIndex(ugp => new { ugp.UserId, ugp.Status });

            // ── Seed: 8 Badge Achievements ────────────────────────────────────────
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement { Id = 101, Name = "Boulder Badge",  Description = "Defeated Norm, Guardian of Daily Life",  ConditionType = ConditionType.GymDefeated, ConditionValue = 1, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 102, Name = "Leaf Badge",     Description = "Defeated Flora, Guardian of Nature",      ConditionType = ConditionType.GymDefeated, ConditionValue = 2, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 103, Name = "Frost Badge",    Description = "Defeated Hail, Guardian of Weather",      ConditionType = ConditionType.GymDefeated, ConditionValue = 3, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 104, Name = "Tide Badge",     Description = "Defeated Marina, Guardian of Food",       ConditionType = ConditionType.GymDefeated, ConditionValue = 4, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 105, Name = "Spark Badge",    Description = "Defeated Volt, Guardian of Technology",   ConditionType = ConditionType.GymDefeated, ConditionValue = 5, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 106, Name = "Wing Badge",     Description = "Defeated Aero, Guardian of Travel",       ConditionType = ConditionType.GymDefeated, ConditionValue = 6, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 107, Name = "Glow Badge",     Description = "Defeated Lumi, Guardian of Health",       ConditionType = ConditionType.GymDefeated, ConditionValue = 7, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 108, Name = "Iron Badge",     Description = "Defeated Brawl, Guardian of Sports",      ConditionType = ConditionType.GymDefeated, ConditionValue = 8, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ── Seed: 8 GymLeaders ────────────────────────────────────────────────
            modelBuilder.Entity<GymLeader>().HasData(
                new GymLeader
                {
                    Id = 1, GymOrder = 1, Name = "Norm",
                    Title = "Guardian of Daily Life",
                    Description = "Norm greets every newcomer with a warm smile. Her words are simple, but mastering them is the foundation of your journey.",
                    BadgeName = "Boulder Badge", BadgeAchievementId = 101,
                    Theme = VocabularySetTheme.DailyLife, RequiredCefrLevel = CEFRLevel.A1,
                    XpThreshold = 300,  VocabThreshold = 15, RequiredMemoryState = "Learning",
                    XpReward = 150, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 2, GymOrder = 2, Name = "Flora",
                    Title = "Guardian of Nature",
                    Description = "Flora speaks in the language of forests and living things. She rewards those who have truly internalized the world around them.",
                    BadgeName = "Leaf Badge", BadgeAchievementId = 102,
                    Theme = VocabularySetTheme.Nature, RequiredCefrLevel = CEFRLevel.A1,
                    XpThreshold = 600,  VocabThreshold = 15, RequiredMemoryState = "Review",
                    XpReward = 200, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 3, GymOrder = 3, Name = "Hail",
                    Title = "Guardian of Weather",
                    Description = "Hail's temperament shifts like the wind. Only those who can describe the sky in all its moods can earn her trust.",
                    BadgeName = "Frost Badge", BadgeAchievementId = 103,
                    Theme = VocabularySetTheme.Weather, RequiredCefrLevel = CEFRLevel.A2,
                    XpThreshold = 1000, VocabThreshold = 20, RequiredMemoryState = "Learning",
                    XpReward = 250, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 4, GymOrder = 4, Name = "Marina",
                    Title = "Guardian of Food",
                    Description = "Marina believes language is best shared over a meal. Prove to her you can navigate any kitchen or restaurant conversation.",
                    BadgeName = "Tide Badge", BadgeAchievementId = 104,
                    Theme = VocabularySetTheme.Food, RequiredCefrLevel = CEFRLevel.A2,
                    XpThreshold = 1500, VocabThreshold = 20, RequiredMemoryState = "Review",
                    XpReward = 300, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 5, GymOrder = 5, Name = "Volt",
                    Title = "Guardian of Technology",
                    Description = "Volt moves at the speed of electricity. Only the digitally fluent can keep up with his rapid-fire tech vocabulary.",
                    BadgeName = "Spark Badge", BadgeAchievementId = 105,
                    Theme = VocabularySetTheme.Technology, RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 2500, VocabThreshold = 25, RequiredMemoryState = "Learning",
                    XpReward = 400, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 6, GymOrder = 6, Name = "Aero",
                    Title = "Guardian of Travel",
                    Description = "Aero has circled the globe many times over. She tests your ability to navigate the world — literally and linguistically.",
                    BadgeName = "Wing Badge", BadgeAchievementId = 106,
                    Theme = VocabularySetTheme.Travel, RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 3500, VocabThreshold = 25, RequiredMemoryState = "Review",
                    XpReward = 500, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 7, GymOrder = 7, Name = "Lumi",
                    Title = "Guardian of Health",
                    Description = "Lumi radiates calm and wisdom. She demands precision — the language of health leaves no room for misunderstanding.",
                    BadgeName = "Glow Badge", BadgeAchievementId = 107,
                    Theme = VocabularySetTheme.Health, RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 5000, VocabThreshold = 30, RequiredMemoryState = "Review",
                    XpReward = 600, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 8, GymOrder = 8, Name = "Brawl",
                    Title = "Guardian of Sports",
                    Description = "Brawl is the ultimate test of endurance. This battle will push your B2 vocabulary to the limit — no shortcuts allowed.",
                    BadgeName = "Iron Badge", BadgeAchievementId = 108,
                    Theme = VocabularySetTheme.Sports, RequiredCefrLevel = CEFRLevel.B2,
                    XpThreshold = 7000, VocabThreshold = 30, RequiredMemoryState = "Learning",
                    XpReward = 800, QuestionCount = 15, PassRatePercent = 80, CooldownHours = 12
                }
            );
    }

}   // class WordSoulDbContext

}   // namespace WordSoul.Infrastructure.Persistence
