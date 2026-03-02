using FluentAssertions;
using Moq;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services
{
    public class UserAchievementServiceTests
    {
        private UserAchievementService CreateService(
            Mock<IUnitOfWork> uowMock,
            Mock<IUserAchievementRepository> userAchievementRepoMock,
            Mock<IAchievementRepository> achievementRepoMock,
            Mock<IUserInventoryService>? inventoryMock = null)
        {
            uowMock.SetupGet(x => x.UserAchievement)
                .Returns(userAchievementRepoMock.Object);

            uowMock.SetupGet(x => x.Achievement)
                .Returns(achievementRepoMock.Object);

            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            return new UserAchievementService(
                uowMock.Object,
                inventoryMock?.Object ?? new Mock<IUserInventoryService>().Object
            );
        }

        [Fact]
        public async Task UpdateAchievementProgress_ShouldComplete_WhenReachTarget()
        {
            var achievement = new Achievement
            {
                Id = 1,
                Name = "Master 10 Words",
                ConditionType = ConditionType.MasterWords,
                ConditionValue = 10
            };

            var userAchievement = new UserAchievement
            {
                AchievementId = 1,
                Achievement = achievement,
                UserId = 1,
                ProgressValue = 5,
                IsCompleted = false
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userAchievementRepoMock = new Mock<IUserAchievementRepository>();
            var achievementRepoMock = new Mock<IAchievementRepository>();
            var inventoryMock = new Mock<IUserInventoryService>();

            achievementRepoMock
                .Setup(x => x.GetAchievementsAsync(
                    ConditionType.MasterWords, 1, int.MaxValue, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Achievement> { achievement });

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userAchievement);

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementByUserAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserAchievement> { userAchievement });

            var service = CreateService(
                uowMock,
                userAchievementRepoMock,
                achievementRepoMock,
                inventoryMock);

            await service.UpdateAchievementProgressAsync(1, ConditionType.MasterWords, 5);

            userAchievement.ProgressValue.Should().Be(10);
            userAchievement.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAchievementProgress_ShouldNotExceedTarget()
        {
            var achievement = new Achievement
            {
                Id = 1,
                Name = "Master 10 Words",
                ConditionType = ConditionType.MasterWords,
                ConditionValue = 10
            };

            var userAchievement = new UserAchievement
            {
                AchievementId = 1,
                Achievement = achievement,
                UserId = 1,
                ProgressValue = 9
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userAchievementRepoMock = new Mock<IUserAchievementRepository>();
            var achievementRepoMock = new Mock<IAchievementRepository>();

            achievementRepoMock
                .Setup(x => x.GetAchievementsAsync(
                    ConditionType.MasterWords, 1, int.MaxValue, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Achievement> { achievement });

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userAchievement);

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementByUserAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserAchievement> { userAchievement });

            var service = CreateService(
                uowMock,
                userAchievementRepoMock,
                achievementRepoMock);

            await service.UpdateAchievementProgressAsync(1, ConditionType.MasterWords, 5);

            userAchievement.ProgressValue.Should().Be(10);
        }

        [Fact]
        public async Task ClaimAchievementReward_ShouldGrantItem()
        {
            var achievement = new Achievement
            {
                Id = 1,
                Name = "Master 10 Words",
                RewardItemId = 200
            };

            var ua = new UserAchievement
            {
                UserId = 1,
                AchievementId = 1,
                Achievement = achievement,
                IsCompleted = true,
                IsClaimed = false
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userAchievementRepoMock = new Mock<IUserAchievementRepository>();
            var achievementRepoMock = new Mock<IAchievementRepository>();
            var inventoryMock = new Mock<IUserInventoryService>();

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ua);

            var service = CreateService(
                uowMock,
                userAchievementRepoMock,
                achievementRepoMock,
                inventoryMock);

            await service.ClaimAchievementRewardAsync(1, 1);

            inventoryMock.Verify(x =>
                x.AddItemToUserAsync(1, 200, 1, It.IsAny<CancellationToken>()),
                Times.Once);

            ua.IsClaimed.Should().BeTrue();
        }

        [Fact]
        public async Task ClaimAchievementReward_ShouldThrow_WhenNotCompleted()
        {
            var ua = new UserAchievement
            {
                UserId = 1,
                AchievementId = 1,
                IsCompleted = false
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userAchievementRepoMock = new Mock<IUserAchievementRepository>();
            var achievementRepoMock = new Mock<IAchievementRepository>();

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ua);

            var service = CreateService(
                uowMock,
                userAchievementRepoMock,
                achievementRepoMock);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ClaimAchievementRewardAsync(1, 1));
        }

        [Fact]
        public async Task ClaimAchievementReward_ShouldThrow_WhenAlreadyClaimed()
        {
            var ua = new UserAchievement
            {
                UserId = 1,
                AchievementId = 1,
                IsCompleted = true,
                IsClaimed = true
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userAchievementRepoMock = new Mock<IUserAchievementRepository>();
            var achievementRepoMock = new Mock<IAchievementRepository>();

            userAchievementRepoMock
                .Setup(x => x.GetUserAchievementAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ua);

            var service = CreateService(
                uowMock,
                userAchievementRepoMock,
                achievementRepoMock);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ClaimAchievementRewardAsync(1, 1));
        }
    }
}