using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Data
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
                .HasIndex(ar => new { ar.LearningSessionId, ar.VocabularyId, ar.QuestionType })
                .IsUnique();


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

        }
    }

}
