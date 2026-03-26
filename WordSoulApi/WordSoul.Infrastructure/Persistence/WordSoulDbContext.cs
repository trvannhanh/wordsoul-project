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
        public DbSet<GymLeaderPet> GymLeaderPets { get; set; }
        public DbSet<UserGymProgress> UserGymProgresses { get; set; }
        public DbSet<BattleSession> BattleSessions { get; set; }
        public DbSet<BattleAnswer> BattleAnswers { get; set; }
        public DbSet<BattleRound> BattleRounds { get; set; }
        public DbSet<BattlePetState> BattlePetStates { get; set; }

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

            // ── BattleRound ─────────────────────────────────
            modelBuilder.Entity<BattleRound>()
                .HasOne(br => br.BattleSession)
                .WithMany(bs => bs.Rounds)
                .HasForeignKey(br => br.BattleSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BattleRound>()
                .HasOne(br => br.Vocabulary)
                .WithMany()
                .HasForeignKey(br => br.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BattleRound>()
                .HasIndex(br => new { br.BattleSessionId, br.RoundIndex })
                .IsUnique();

            // ── BattlePetState ─────────────────────────────
            modelBuilder.Entity<BattlePetState>()
                .HasOne(bps => bps.BattleSession)
                .WithMany(bs => bs.PetStates)
                .HasForeignKey(bps => bps.BattleSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BattlePetState>()
                .HasOne(bps => bps.UserOwnedPet)
                .WithMany()
                .HasForeignKey(bps => bps.UserOwnedPetId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<BattlePetState>()
                .HasOne(bps => bps.GymLeaderPet)
                .WithMany()
                .HasForeignKey(bps => bps.GymLeaderPetId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ── GymLeaderPet ───────────────────────────────
            modelBuilder.Entity<GymLeaderPet>()
                .HasOne(glp => glp.GymLeader)
                .WithMany(gl => gl.GymLeaderPets)
                .HasForeignKey(glp => glp.GymLeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GymLeaderPet>()
                .HasOne(glp => glp.Pet)
                .WithMany()
                .HasForeignKey(glp => glp.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GymLeaderPet>()
                .HasIndex(glp => new { glp.GymLeaderId, glp.SlotIndex })
                .IsUnique();

            // Indexes
            modelBuilder.Entity<BattleSession>()
                .HasIndex(bs => new { bs.ChallengerUserId, bs.Status });
            modelBuilder.Entity<UserGymProgress>()
                .HasIndex(ugp => new { ugp.UserId, ugp.Status });

            // ── Seed: 8 Badge Achievements ────────────────────────────────────────
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement { Id = 101, Name = "Boulder Badge", Description = "Defeated Brock, Pewter City Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 1, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 102, Name = "Cascade Badge", Description = "Defeated Misty, Cerulean City Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 2, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 103, Name = "Thunder Badge", Description = "Defeated Lt. Surge, Vermilion Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 3, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 104, Name = "Rainbow Badge", Description = "Defeated Erika, Celadon Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 4, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 105, Name = "Soul Badge", Description = "Defeated Koga, Fuchsia Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 5, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 106, Name = "Marsh Badge", Description = "Defeated Sabrina, Saffron Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 6, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 107, Name = "Volcano Badge", Description = "Defeated Blaine, Cinnabar Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 7, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Achievement { Id = 108, Name = "Earth Badge", Description = "Defeated Giovanni, Viridian Gym Leader", ConditionType = ConditionType.GymDefeated, ConditionValue = 8, RewardItemId = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ── Seed: 8 GymLeaders ────────────────────────────────────────────────
            modelBuilder.Entity<GymLeader>().HasData(
                new GymLeader
                {
                    Id = 1,
                    GymOrder = 1,
                    Name = "Brock",
                    Title = "Rock-Type Master",
                    Description = "Brock tests your fundamentals. Solid like rock, his battle demands strong basics.",
                    BadgeName = "Boulder Badge",
                    BadgeAchievementId = 101,
                    Theme = VocabularySetTheme.Challenge,
                    RequiredCefrLevel = CEFRLevel.A1,
                    XpThreshold = 300,
                    VocabThreshold = 15,
                    RequiredMemoryState = "Learning",
                    XpReward = 150,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 2,
                    GymOrder = 2,
                    Name = "Misty",
                    Title = "Water-Type Specialist",
                    Description = "Misty flows like water. Adaptability is key to overcoming her strategies.",
                    BadgeName = "Cascade Badge",
                    BadgeAchievementId = 102,
                    Theme = VocabularySetTheme.Food,
                    RequiredCefrLevel = CEFRLevel.A1,
                    XpThreshold = 600,
                    VocabThreshold = 15,
                    RequiredMemoryState = "Review",
                    XpReward = 200,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 3,
                    GymOrder = 3,
                    Name = "Lt. Surge",
                    Title = "Lightning American",
                    Description = "Fast and explosive, Lt. Surge overwhelms unprepared challengers.",
                    BadgeName = "Thunder Badge",
                    BadgeAchievementId = 103,
                    Theme = VocabularySetTheme.Technology,
                    RequiredCefrLevel = CEFRLevel.A2,
                    XpThreshold = 1000,
                    VocabThreshold = 20,
                    RequiredMemoryState = "Learning",
                    XpReward = 250,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 4,
                    GymOrder = 4,
                    Name = "Erika",
                    Title = "Nature-Loving Princess",
                    Description = "Erika’s calm style hides dangerous precision. Patience wins this match.",
                    BadgeName = "Rainbow Badge",
                    BadgeAchievementId = 104,
                    Theme = VocabularySetTheme.Nature,
                    RequiredCefrLevel = CEFRLevel.A2,
                    XpThreshold = 1500,
                    VocabThreshold = 20,
                    RequiredMemoryState = "Review",
                    XpReward = 300,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 5,
                    GymOrder = 5,
                    Name = "Koga",
                    Title = "Poison Ninja Master",
                    Description = "Koga uses deception and speed. One mistake and it's over.",
                    BadgeName = "Soul Badge",
                    BadgeAchievementId = 105,
                    Theme = VocabularySetTheme.Poison,
                    RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 2500,
                    VocabThreshold = 25,
                    RequiredMemoryState = "Learning",
                    XpReward = 400,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 6,
                    GymOrder = 6,
                    Name = "Sabrina",
                    Title = "Psychic Master",
                    Description = "Sabrina reads your moves before you make them. Precision is mandatory.",
                    BadgeName = "Marsh Badge",
                    BadgeAchievementId = 106,
                    Theme = VocabularySetTheme.Science,
                    RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 3500,
                    VocabThreshold = 25,
                    RequiredMemoryState = "Review",
                    XpReward = 500,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 7,
                    GymOrder = 7,
                    Name = "Blaine",
                    Title = "Fire-Type Quiz Master",
                    Description = "Blaine combines knowledge and battle. Expect tricky questions and heat.",
                    BadgeName = "Volcano Badge",
                    BadgeAchievementId = 107,
                    Theme = VocabularySetTheme.Custom,
                    RequiredCefrLevel = CEFRLevel.B1,
                    XpThreshold = 5000,
                    VocabThreshold = 30,
                    RequiredMemoryState = "Review",
                    XpReward = 600,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                },
                new GymLeader
                {
                    Id = 8,
                    GymOrder = 8,
                    Name = "Giovanni",
                    Title = "Team Rocket Boss",
                    Description = "Giovanni is the ultimate test. Strategy, power, and mastery decide victory.",
                    BadgeName = "Earth Badge",
                    BadgeAchievementId = 108,
                    Theme = VocabularySetTheme.Challenge,
                    RequiredCefrLevel = CEFRLevel.B2,
                    XpThreshold = 7000,
                    VocabThreshold = 30,
                    RequiredMemoryState = "Learning",
                    XpReward = 800,
                    QuestionCount = 15,
                    PassRatePercent = 80,
                    CooldownHours = 12
                }
            );

            // ── Seed: GymLeaderPets (3 per Gym) ──────────────────────────────────
            // PetId mapping: PetId = (GymOrder - 1) * 3 + SlotIndex + 1
            // BotAccuracy tăng dần theo Gym: Gym1=0.55, Gym8=0.90
            // BotAvgResponseMs giảm dần: Gym1=7000ms, Gym8=3000ms
            modelBuilder.Entity<GymLeaderPet>().HasData(
                // Gym 1 – Norm (Dễ: Normal)
                new GymLeaderPet { Id = 1,  GymLeaderId = 1, PetId = 50,  SlotIndex = 0, BotAccuracy = 0.55, BotAvgResponseMs = 7000 },
                new GymLeaderPet { Id = 2,  GymLeaderId = 1, PetId = 51,  SlotIndex = 1, BotAccuracy = 0.55, BotAvgResponseMs = 7000 },
                new GymLeaderPet { Id = 3,  GymLeaderId = 1, PetId = 52,  SlotIndex = 2, BotAccuracy = 0.55, BotAvgResponseMs = 7000 },
                // Gym 2 – Flora
                new GymLeaderPet { Id = 4,  GymLeaderId = 2, PetId = 53,  SlotIndex = 0, BotAccuracy = 0.60, BotAvgResponseMs = 6500 },
                new GymLeaderPet { Id = 5,  GymLeaderId = 2, PetId = 54,  SlotIndex = 1, BotAccuracy = 0.60, BotAvgResponseMs = 6500 },
                new GymLeaderPet { Id = 6,  GymLeaderId = 2, PetId = 55,  SlotIndex = 2, BotAccuracy = 0.60, BotAvgResponseMs = 6500 },
                // Gym 3 – Hail
                new GymLeaderPet { Id = 7,  GymLeaderId = 3, PetId = 56,  SlotIndex = 0, BotAccuracy = 0.65, BotAvgResponseMs = 6000 },
                new GymLeaderPet { Id = 8,  GymLeaderId = 3, PetId = 57,  SlotIndex = 1, BotAccuracy = 0.65, BotAvgResponseMs = 6000 },
                new GymLeaderPet { Id = 9,  GymLeaderId = 3, PetId = 58,  SlotIndex = 2, BotAccuracy = 0.65, BotAvgResponseMs = 6000 },
                // Gym 4 – Marina
                new GymLeaderPet { Id = 10, GymLeaderId = 4, PetId = 59, SlotIndex = 0, BotAccuracy = 0.70, BotAvgResponseMs = 5500 },
                new GymLeaderPet { Id = 11, GymLeaderId = 4, PetId = 60, SlotIndex = 1, BotAccuracy = 0.70, BotAvgResponseMs = 5500 },
                new GymLeaderPet { Id = 12, GymLeaderId = 4, PetId = 61, SlotIndex = 2, BotAccuracy = 0.70, BotAvgResponseMs = 5500 },
                // Gym 5 – Volt
                new GymLeaderPet { Id = 13, GymLeaderId = 5, PetId = 62, SlotIndex = 0, BotAccuracy = 0.75, BotAvgResponseMs = 5000 },
                new GymLeaderPet { Id = 14, GymLeaderId = 5, PetId = 63, SlotIndex = 1, BotAccuracy = 0.75, BotAvgResponseMs = 5000 },
                new GymLeaderPet { Id = 15, GymLeaderId = 5, PetId = 64, SlotIndex = 2, BotAccuracy = 0.75, BotAvgResponseMs = 5000 },
                // Gym 6 – Aero
                new GymLeaderPet { Id = 16, GymLeaderId = 6, PetId = 65, SlotIndex = 0, BotAccuracy = 0.80, BotAvgResponseMs = 4500 },
                new GymLeaderPet { Id = 17, GymLeaderId = 6, PetId = 66, SlotIndex = 1, BotAccuracy = 0.80, BotAvgResponseMs = 4500 },
                new GymLeaderPet { Id = 18, GymLeaderId = 6, PetId = 67, SlotIndex = 2, BotAccuracy = 0.80, BotAvgResponseMs = 4500 },
                // Gym 7 – Lumi
                new GymLeaderPet { Id = 19, GymLeaderId = 7, PetId = 68, SlotIndex = 0, BotAccuracy = 0.85, BotAvgResponseMs = 3500 },
                new GymLeaderPet { Id = 20, GymLeaderId = 7, PetId = 69, SlotIndex = 1, BotAccuracy = 0.85, BotAvgResponseMs = 3500 },
                new GymLeaderPet { Id = 21, GymLeaderId = 7, PetId = 70, SlotIndex = 2, BotAccuracy = 0.85, BotAvgResponseMs = 3500 },
                // Gym 8 – Brawl (Khó nhất)
                new GymLeaderPet { Id = 22, GymLeaderId = 8, PetId = 71, SlotIndex = 0, BotAccuracy = 0.90, BotAvgResponseMs = 3000 },
                new GymLeaderPet { Id = 23, GymLeaderId = 8, PetId = 72, SlotIndex = 1, BotAccuracy = 0.90, BotAvgResponseMs = 3000 },
                new GymLeaderPet { Id = 24, GymLeaderId = 8, PetId = 73, SlotIndex = 2, BotAccuracy = 0.90, BotAvgResponseMs = 3000 }
            );
        }

}   // class WordSoulDbContext

}   // namespace WordSoul.Infrastructure.Persistence
