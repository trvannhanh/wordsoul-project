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
        public DbSet<BattleRound> BattleRounds { get; set; }
        public DbSet<BattlePetState> BattlePetStates { get; set; }

        // ── System Configuration ──────────────────────────────
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        // ── User Groups ──────────────────────────────────
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserGroupMember> UserGroupMembers { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        // ── Pronunciation Practice ─────────────────────────────────
        public DbSet<PronunciationAttempt> PronunciationAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can go here

            modelBuilder.Entity<ActivityLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // Cascade: deleting user removes their activity logs

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
                .OnDelete(DeleteBehavior.Cascade); // Cascade: deleting a session removes its answer records


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

            // ── External Login ────────────────────────────────────
            // Unique index chỉ áp dụng khi Provider không null (filtered index)
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.ExternalLoginProvider, u.ExternalLoginProviderKey })
                .IsUnique()
                .HasFilter("[ExternalLoginProvider] IS NOT NULL");
            // ── User Groups ─────────────────────────────────────────
            modelBuilder.Entity<UserGroupMember>()
                .HasKey(m => new { m.UserGroupId, m.UserId });

            modelBuilder.Entity<UserGroupMember>()
                .HasOne(m => m.UserGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGroupMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGroup>()
                .HasOne(g => g.CreatedByUser)
                .WithMany()
                .HasForeignKey(g => g.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserGroup>()
                .HasIndex(g => g.Name);

            // ── PronunciationAttempt ─────────────────────────────────────────
            modelBuilder.Entity<PronunciationAttempt>()
                .HasOne(pa => pa.User)
                .WithMany()
                .HasForeignKey(pa => pa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PronunciationAttempt>()
                .HasOne(pa => pa.Vocabulary)
                .WithMany()
                .HasForeignKey(pa => pa.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict); // Giữ lịch sử phát âm kể cả khi xoá từ vựng

            // Index để tối ưu query lịch sử phát âm theo user + từ
            modelBuilder.Entity<PronunciationAttempt>()
                .HasIndex(pa => new { pa.UserId, pa.VocabularyId, pa.AttemptTime });

            // Index cho achievement check (count Perfect theo userId)
            modelBuilder.Entity<PronunciationAttempt>()
                .HasIndex(pa => new { pa.UserId, pa.Result });

            // ── System Configuration Seeding ─────────────────────────────────
            var seedTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<SystemConfiguration>().HasData(
                new SystemConfiguration { Key = "SrsMinEf",              Value = "1.3",   DataType = "Float",   Category = "SRS",          Description = "Minimum Ease Factor for SM-2 Algorithm",                                             LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "SrsInitialInterval1",   Value = "1",     DataType = "Integer", Category = "SRS",          Description = "First interval (days) for SM-2",                                                    LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "SrsInitialInterval2",   Value = "6",     DataType = "Integer", Category = "SRS",          Description = "Second interval (days) for SM-2",                                                   LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "CatchRateWrongPenalty", Value = "0.05",  DataType = "Float",   Category = "GAME_BALANCE", Description = "Penalty applied to catch rate for each wrong answer (e.g. 0.05 = 5%)",           LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "XpRewardNewSession",    Value = "20",    DataType = "Integer", Category = "GAME_BALANCE", Description = "XP rewarded for completing a learning session with new words",                  LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "XpRewardReviewSession", Value = "100",   DataType = "Integer", Category = "GAME_BALANCE", Description = "XP rewarded for completing a review session",                                   LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                // General Settings
                new SystemConfiguration { Key = "AllowRegistration",     Value = "true",  DataType = "Boolean", Category = "GENERAL",      Description = "Allow new users to register on the platform",                                 LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "MaintenanceMode",       Value = "false", DataType = "Boolean", Category = "GENERAL",      Description = "Show maintenance notice to regular users (does not affect admins)",           LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "MaxGroupSize",          Value = "50",    DataType = "Integer", Category = "GENERAL",      Description = "Maximum number of members allowed in a single user group",                    LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "AppDisplayName",        Value = "VocaMon", DataType = "String", Category = "GENERAL",     Description = "Application display name shown to users in the UI",                          LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                
                // Admin branding configurations
                new SystemConfiguration { Key = "AdminAppName",        Value = "VocaMon Admin", DataType = "String", Category = "GENERAL",     Description = "Application name for the Admin portal",                       LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "AdminAppLogo",        Value = "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png", DataType = "String", Category = "GENERAL",     Description = "Logo URL for the Admin portal",                               LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                
                // Web App branding configurations
                new SystemConfiguration { Key = "WebAppName",          Value = "VocaMon", DataType = "String", Category = "GENERAL",     Description = "Application name for the client Web App",                    LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "WebAppSubtitle",      Value = "Học từ vựng cùng thú cưng", DataType = "String", Category = "GENERAL",     Description = "Subtitle/Slogan for the client Web App",                     LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "WebAppLogo",          Value = "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png", DataType = "String", Category = "GENERAL",     Description = "Logo URL for the client Web App",                            LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "WebAppFavicon",       Value = "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png", DataType = "String", Category = "GENERAL",     Description = "Favicon URL for the client Web App",                         LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                
                // Recommended configurations
                new SystemConfiguration { Key = "ContactEmail",        Value = "support@vocamon.online", DataType = "String", Category = "GENERAL",     Description = "Support/contact email address shown to users",               LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "AllowGoogleLogin",     Value = "true",  DataType = "Boolean", Category = "GENERAL",      Description = "Enable or disable Google OAuth registration and login",       LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "FooterCopyright",      Value = "© 2026 VocaMon. All rights reserved.", DataType = "String", Category = "GENERAL",     Description = "Copyright text shown in the Web App footer",                 LastUpdatedBy = "System", LastUpdatedAt = seedTime },
                new SystemConfiguration { Key = "FacebookUrl",          Value = "https://www.facebook.com/giidavibe/", DataType = "String", Category = "GENERAL",     Description = "Official Facebook Fanpage link",                            LastUpdatedBy = "System", LastUpdatedAt = seedTime },

                // System Settings
                new SystemConfiguration { Key = "LogRetentionDays",      Value = "7",     DataType = "Integer", Category = "SYSTEM",       Description = "Number of days to keep system logs before auto-deleting",                     LastUpdatedBy = "System", LastUpdatedAt = seedTime }
            );
        }

}   // class WordSoulDbContext

}   // namespace WordSoul.Infrastructure.Persistence
