using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Tests.Services
{
    public class DailyQuestServiceTests
    {
        private (DailyQuestService service, WordSoulDbContext context)
            CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<WordSoulDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var context = new WordSoulDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new DailyQuestService(uow);

            return (service, context);
        }

        [Fact]
        public async Task GenerateDailyQuests_ShouldCreateQuests_WhenNotExist()
        {
            var (service, context) =
                CreateService(nameof(GenerateDailyQuests_ShouldCreateQuests_WhenNotExist));

            // Seed 4 quest templates
            context.DailyQuests.AddRange(
                new DailyQuest { Id = 1, Title = "Learn 10", QuestType = Domain.Enums.QuestType.Learn, TargetValue = 10, RewardType = 0, RewardValue = 100 },
                new DailyQuest { Id = 2, Title = "Review 20", QuestType = Domain.Enums.QuestType.Review, TargetValue = 20, RewardType = 0, RewardValue = 150 }
            );

            await context.SaveChangesAsync();

            await service.GenerateDailyQuestsForUserAsync(1);

            var today = DateTime.UtcNow.Date;

            var userQuests = context.UserDailyQuests
                .Where(x => x.UserId == 1 && x.QuestDate == today)
                .ToList();

            Assert.Equal(2, userQuests.Count);
            Assert.All(userQuests, q => Assert.False(q.IsCompleted));
            Assert.All(userQuests, q => Assert.Equal(0, q.Progress));
        }

        [Fact]
        public async Task GenerateDailyQuests_ShouldNotDuplicate_WhenAlreadyExists()
        {
            var (service, context) =
                CreateService(nameof(GenerateDailyQuests_ShouldNotDuplicate_WhenAlreadyExists));

            var today = DateTime.UtcNow.Date;

            context.DailyQuests.Add(
                new DailyQuest { Id = 1, Title = "Learn 10", QuestType = Domain.Enums.QuestType.Learn, TargetValue = 10, RewardType = 0, RewardValue = 100 });

            context.UserDailyQuests.Add(
                new UserDailyQuest
                {
                    UserId = 1,
                    DailyQuestId = 1,
                    QuestDate = today,
                    Progress = 5
                });

            await context.SaveChangesAsync();

            await service.GenerateDailyQuestsForUserAsync(1);

            var userQuests = context.UserDailyQuests
                .Where(x => x.UserId == 1 && x.QuestDate == today)
                .ToList();

            Assert.Single(userQuests);
            Assert.Equal(5, userQuests[0].Progress); // đảm bảo không reset
        }


        [Fact]
        public async Task GetUserDailyQuests_ShouldReturnOnlySpecificDate()
        {
            var (service, context) =
                CreateService(nameof(GetUserDailyQuests_ShouldReturnOnlySpecificDate));

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            context.DailyQuests.AddRange(
                new DailyQuest { Id = 1, Title = "Q1", QuestType = Domain.Enums.QuestType.Learn, TargetValue = 1, RewardType = 0, RewardValue = 10 },
                new DailyQuest { Id = 2, Title = "Q2", QuestType = Domain.Enums.QuestType.Learn, TargetValue = 1, RewardType = 0, RewardValue = 10 }
            );

            context.UserDailyQuests.AddRange(
                new UserDailyQuest { UserId = 1, DailyQuestId = 1, QuestDate = today },
                new UserDailyQuest { UserId = 1, DailyQuestId = 2, QuestDate = yesterday }
            );

            await context.SaveChangesAsync();

            await context.SaveChangesAsync();

            var result = await service.GetUserDailyQuestsAsync(1, today);

            Assert.Single(result);
            Assert.Equal(today, result[0].QuestDate);
        }
    }
}
