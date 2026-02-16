using FluentAssertions;
using Moq;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services
{
    public class UserInventoryServiceTests
    {
        private UserInventoryService CreateService(
            Mock<IUnitOfWork> uowMock,
            Mock<IUserItemRepository> userItemRepoMock)
        {
            uowMock.SetupGet(x => x.UserItem)
                .Returns(userItemRepoMock.Object);

            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            return new UserInventoryService(uowMock.Object);
        }

        [Fact]
        public async Task AddItemToUserAsync_ShouldCreateNewItem_WhenNotExist()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var userItemRepoMock = new Mock<IUserItemRepository>();

            userItemRepoMock
                .Setup(x => x.GetUserItemAsync(1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserItem?)null);

            var service = CreateService(uowMock, userItemRepoMock);

            await service.AddItemToUserAsync(1, 100, 3);

            userItemRepoMock.Verify(x =>
                x.CreateUserItemAsync(It.Is<UserItem>(ui =>
                    ui.UserId == 1 &&
                    ui.ItemId == 100 &&
                    ui.Quantity == 3),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddItemToUserAsync_ShouldIncreaseQuantity_WhenItemExists()
        {
            var existing = new UserItem
            {
                UserId = 1,
                ItemId = 100,
                Quantity = 5
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userItemRepoMock = new Mock<IUserItemRepository>();

            userItemRepoMock
                .Setup(x => x.GetUserItemAsync(1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var service = CreateService(uowMock, userItemRepoMock);

            await service.AddItemToUserAsync(1, 100, 3);

            existing.Quantity.Should().Be(8);

            userItemRepoMock.Verify(x =>
                x.UpdateUserItemAsync(existing, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveItemFromUserAsync_ShouldDecreaseQuantity()
        {
            var existing = new UserItem
            {
                UserId = 1,
                ItemId = 100,
                Quantity = 10
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userItemRepoMock = new Mock<IUserItemRepository>();

            userItemRepoMock
                .Setup(x => x.GetUserItemAsync(1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var service = CreateService(uowMock, userItemRepoMock);

            await service.RemoveItemFromUserAsync(1, 100, 4);

            existing.Quantity.Should().Be(6);

            userItemRepoMock.Verify(x =>
                x.UpdateUserItemAsync(existing, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveItemFromUserAsync_ShouldThrow_WhenNotEnoughItems()
        {
            var existing = new UserItem
            {
                UserId = 1,
                ItemId = 100,
                Quantity = 2
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userItemRepoMock = new Mock<IUserItemRepository>();

            userItemRepoMock
                .Setup(x => x.GetUserItemAsync(1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var service = CreateService(uowMock, userItemRepoMock);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RemoveItemFromUserAsync(1, 100, 5));
        }

        [Fact]
        public async Task GetUserInventoryAsync_ShouldReturnItems()
        {
            var items = new List<UserItem>
            {
                new UserItem { UserId = 1, ItemId = 100, Quantity = 3 },
                new UserItem { UserId = 1, ItemId = 200, Quantity = 5 }
            };

            var uowMock = new Mock<IUnitOfWork>();
            var userItemRepoMock = new Mock<IUserItemRepository>();

            userItemRepoMock
                .Setup(x => x.GetUserItemsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            var service = CreateService(uowMock, userItemRepoMock);

            var result = await service.GetUserInventoryAsync(1);

            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(items);
        }
    }
}