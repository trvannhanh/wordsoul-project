using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
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

            var inventoryMock = new Mock<IUserInventoryService>();
            var loggerMock = new Mock<ILogger<DailyQuestService>>();

            var service = new DailyQuestService(
                uow,
                inventoryMock.Object,
                loggerMock.Object
            );

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


            var result = await service.GetUserDailyQuestsAsync(1, today);

            Assert.Single(result);
            Assert.Equal(today, result[0].QuestDate);
        }


        [Fact]
        public async Task UpdateQuestProgress_ShouldBeThreadSafe()
        {
            // Arrange
            var (service, context) = CreateService(nameof(UpdateQuestProgress_ShouldBeThreadSafe));

            var today = DateTime.UtcNow.Date;

            var template = new DailyQuest
            {
                Id = 1,
                Title = "Learn 10",
                QuestType = QuestType.Learn,
                TargetValue = 10,
                IsActive = true
            };

            context.DailyQuests.Add(template);

            context.UserDailyQuests.Add(new UserDailyQuest
            {
                UserId = 1,
                DailyQuestId = 1,
                DailyQuest = template,
                Progress = 0,
                IsCompleted = false,
                QuestDate = today
            });

            await context.SaveChangesAsync();

            // Act
            var tasks = Enumerable.Range(0, 50)
                .Select(_ => service.UpdateQuestProgressAsync(1, QuestType.Learn));

            await Task.WhenAll(tasks);

            var result = await context.UserDailyQuests.FirstAsync();

            // Assert
            Assert.Equal(10, result.Progress);
            Assert.True(result.IsCompleted);
        }

        [Fact]
        public async Task UpdateQuestProgress_ShouldNotLoseIncrements()
        {
            var (service, context) = CreateService(nameof(UpdateQuestProgress_ShouldNotLoseIncrements));

            var today = DateTime.UtcNow.Date;

            var template = new DailyQuest
            {
                Id = 1,
                Title = "Learn 10",
                QuestType = QuestType.Learn,
                TargetValue = 100,
                IsActive = true
            };

            context.DailyQuests.Add(template);

            context.UserDailyQuests.Add(new UserDailyQuest
            {
                UserId = 1,
                DailyQuestId = 1,
                DailyQuest = template,
                Progress = 0,
                IsCompleted = false,
                QuestDate = today
            });

            await context.SaveChangesAsync();

            var tasks = Enumerable.Range(0, 50)
                .Select(_ => service.UpdateQuestProgressAsync(1, QuestType.Learn));

            await Task.WhenAll(tasks);

            var result = await context.UserDailyQuests.FirstAsync();

            Assert.Equal(50, result.Progress);
        }
    }
}
