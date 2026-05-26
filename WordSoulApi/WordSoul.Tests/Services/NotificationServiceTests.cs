using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services;

public class NotificationServiceTests
{
    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<INotificationRepository>      NotifRepo,
        Mock<IRealtimeNotificationService> Realtime,
        Mock<IUnitOfWork>                  Uow,
        Mock<IUserRepository>              UserRepo,
        Mock<IActivityLogService>          ActivityLog);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (NotificationService service, Deps deps) CreateService()
    {
        var notifRepo   = new Mock<INotificationRepository>();
        var realtime    = new Mock<IRealtimeNotificationService>();
        var uow         = new Mock<IUnitOfWork>();
        var userRepo    = new Mock<IUserRepository>();
        var activityLog = new Mock<IActivityLogService>();
        var fcmService  = new Mock<IFcmService>();

        uow.Setup(u => u.User).Returns(userRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        notifRepo.Setup(r => r.CreateNotificationAsync(
                It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        notifRepo.Setup(r => r.MarkAsReadNotificationAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        notifRepo.Setup(r => r.MarkAllAsReadAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        notifRepo.Setup(r => r.DeleteNotificationAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        realtime.Setup(r => r.SendNotificationAsync(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        activityLog.Setup(s => s.CreateActivityLogAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new NotificationService(
            notifRepo.Object,
            realtime.Object,
            uow.Object,
            activityLog.Object,
            new Mock<ILogger<NotificationService>>().Object,
            fcmService.Object);

        return (service, new Deps(notifRepo, realtime, uow, userRepo, activityLog));
    }

    // ─── Entity helpers ────────────────────────────────────────────────────────
    private static Notification MakeNotification(int id = 1, int userId = 1, bool isRead = false) =>
        new Notification { Id = id, UserId = userId, Title = "Title", Message = "Msg", IsRead = isRead };

    private static User MakeUser(int id) => new User { Id = id, Email = $"u{id}@test.com" };

    // ============================================================================
    // CreateNotificationAsync
    // ============================================================================

    [Fact]
    public async Task Create_SavesAndSendsRealtime()
    {
        var (service, deps) = CreateService();

        await service.CreateNotificationAsync(1, "Hello", "World", NotificationType.Review);

        deps.NotifRepo.Verify(
            r => r.CreateNotificationAsync(
                It.Is<Notification>(n => n.UserId == 1 && n.Title == "Hello"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        deps.Realtime.Verify(r => r.SendNotificationAsync(1, It.IsAny<object>()), Times.Once);
    }

    // ============================================================================
    // BroadcastAsync
    // ============================================================================

    [Fact]
    public async Task Broadcast_WithSpecificTargets_SendsOnlyToThoseUsers()
    {
        var (service, deps) = CreateService();
        var dto = new BroadcastNotificationDto
        {
            Title = "News",
            Message = "Body",
            TargetUserIds = [10, 20]
        };

        var result = await service.BroadcastAsync(dto, adminUserId: 99);

        result.NotificationsSent.Should().Be(2);
        deps.NotifRepo.Verify(
            r => r.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        // Should NOT call GetAllUsersAsync because targets were specified
        deps.UserRepo.Verify(
            r => r.GetAllUsersAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<UserRole?>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Broadcast_NoTargets_FetchesAllUsersAndSendsToEach()
    {
        var (service, deps) = CreateService();
        deps.UserRepo
            .Setup(r => r.GetAllUsersAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<UserRole?>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([MakeUser(1), MakeUser(2), MakeUser(3)]);
        var dto = new BroadcastNotificationDto { Title = "All", Message = "Msg", TargetUserIds = null };

        var result = await service.BroadcastAsync(dto, adminUserId: 99);

        result.NotificationsSent.Should().Be(3);
        deps.NotifRepo.Verify(
            r => r.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Broadcast_LogsActivityWithAdminUserId()
    {
        var (service, deps) = CreateService();
        var dto = new BroadcastNotificationDto
        {
            Title = "Test",
            Message = "Msg",
            TargetUserIds = [5]
        };

        await service.BroadcastAsync(dto, adminUserId: 42);

        deps.ActivityLog.Verify(
            s => s.CreateActivityLogAsync(
                42,
                "ADMIN_BROADCAST",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ============================================================================
    // GetUserNotificationsAsync
    // ============================================================================

    [Fact]
    public async Task GetUserNotifications_NoItems_ReturnsEmpty()
    {
        var (service, deps) = CreateService();
        deps.NotifRepo.Setup(r => r.GetUserNotificationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await service.GetUserNotificationsAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserNotifications_WithItems_ReturnsMappedDtos()
    {
        var (service, deps) = CreateService();
        deps.NotifRepo.Setup(r => r.GetUserNotificationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([MakeNotification(1, userId: 1), MakeNotification(2, userId: 1, isRead: true)]);

        var result = await service.GetUserNotificationsAsync(1);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(n => n.Id == 1 && !n.IsRead);
        result.Should().ContainSingle(n => n.Id == 2 && n.IsRead);
    }

    // ============================================================================
    // MarkAsReadNotificationAsync
    // ============================================================================

    [Fact]
    public async Task MarkAsRead_CallsRepoAndSaves()
    {
        var (service, deps) = CreateService();

        await service.MarkAsReadNotificationAsync(7);

        deps.NotifRepo.Verify(
            r => r.MarkAsReadNotificationAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // MarkAllAsReadAsync
    // ============================================================================

    [Fact]
    public async Task MarkAllAsRead_CallsRepoAndSaves()
    {
        var (service, deps) = CreateService();

        await service.MarkAllAsReadAsync(userId: 3);

        deps.NotifRepo.Verify(
            r => r.MarkAllAsReadAsync(3, It.IsAny<CancellationToken>()), Times.Once);
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // DeleteNotificationAsync
    // ============================================================================

    [Fact]
    public async Task Delete_NotificationNotFound_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        deps.NotifRepo.Setup(r => r.GetNotificationByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var act = () => service.DeleteNotificationAsync(id: 1, currentUserId: 5);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_WrongOwner_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        // notification owned by user 10, not user 5
        deps.NotifRepo.Setup(r => r.GetNotificationByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeNotification(1, userId: 10));

        var act = () => service.DeleteNotificationAsync(id: 1, currentUserId: 5);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_CorrectOwner_DeletesAndSaves()
    {
        var (service, deps) = CreateService();
        deps.NotifRepo.Setup(r => r.GetNotificationByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeNotification(1, userId: 5));

        await service.DeleteNotificationAsync(id: 1, currentUserId: 5);

        deps.NotifRepo.Verify(
            r => r.DeleteNotificationAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
