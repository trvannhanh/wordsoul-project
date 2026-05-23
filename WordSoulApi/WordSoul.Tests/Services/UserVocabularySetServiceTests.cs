using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services;

public class UserVocabularySetServiceTests
{
    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<IUnitOfWork>                  Uow,
        Mock<IUserRepository>              UserRepo,
        Mock<IVocabularySetRepository>     VocabSetRepo,
        Mock<IUserVocabularySetRepository> UserVocabSetRepo);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (UserVocabularySetService service, Deps deps) CreateService()
    {
        var uow              = new Mock<IUnitOfWork>();
        var userRepo         = new Mock<IUserRepository>();
        var vocabSetRepo     = new Mock<IVocabularySetRepository>();
        var userVocabSetRepo = new Mock<IUserVocabularySetRepository>();

        uow.Setup(u => u.User).Returns(userRepo.Object);
        uow.Setup(u => u.VocabularySet).Returns(vocabSetRepo.Object);
        uow.Setup(u => u.UserVocabularySet).Returns(userVocabSetRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        userVocabSetRepo
            .Setup(r => r.AddVocabularySetToUserAsync(
                It.IsAny<UserVocabularySet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserVocabularySetService(
            uow.Object,
            new Mock<ILogger<UserVocabularySetService>>().Object);

        return (service, new Deps(uow, userRepo, vocabSetRepo, userVocabSetRepo));
    }

    // ─── Entity helpers ────────────────────────────────────────────────────────
    private static User MakeUser(int id = 1) =>
        new User { Id = id, Email = $"u{id}@test.com" };

    private static VocabularySet MakeSet(int id = 1, int? createdById = null, bool isPublic = true) =>
        new VocabularySet { Id = id, Title = "Test Set", CreatedById = createdById, IsPublic = isPublic };

    private static UserVocabularySet MakeRelation(int userId = 1, int setId = 1) =>
        new UserVocabularySet
        {
            UserId = userId,
            VocabularySetId = setId,
            IsActive = true,
            IsCompleted = false,
            TotalCompletedSession = 3,
            CreatedAt = DateTime.UtcNow
        };

    // ============================================================================
    // AddVocabularySetToUserAsync
    // ============================================================================

    [Fact]
    public async Task Add_UserNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => service.AddVocabularySetToUserAsync(1, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User*1*");
    }

    [Fact]
    public async Task Add_SetNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);

        var act = () => service.AddVocabularySetToUserAsync(1, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*VocabularySet*10*");
    }

    [Fact]
    public async Task Add_PrivateSetNotOwner_ThrowsInvalidOperationException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        // owned by user 99, not public
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSet(id: 10, createdById: 99, isPublic: false));

        var act = () => service.AddVocabularySetToUserAsync(userId: 1, vocabSetId: 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*private*");
    }

    [Fact]
    public async Task Add_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSet(id: 10, isPublic: true));
        deps.UserVocabSetRepo
            .Setup(r => r.CheckUserHasVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => service.AddVocabularySetToUserAsync(1, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task Add_PublicSet_Success_SavesRelation()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSet(id: 10, isPublic: true));
        deps.UserVocabSetRepo
            .Setup(r => r.CheckUserHasVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await service.AddVocabularySetToUserAsync(1, 10);

        deps.UserVocabSetRepo.Verify(
            r => r.AddVocabularySetToUserAsync(
                It.Is<UserVocabularySet>(uvs => uvs.UserId == 1 && uvs.VocabularySetId == 10 && uvs.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Add_PrivateSetOwner_Success_SavesRelation()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(5));
        // private but user 5 is the owner
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSet(id: 10, createdById: 5, isPublic: false));
        deps.UserVocabSetRepo
            .Setup(r => r.CheckUserHasVocabularySetAsync(5, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await service.AddVocabularySetToUserAsync(userId: 5, vocabSetId: 10);

        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // GetUserVocabularySetAsync
    // ============================================================================

    [Fact]
    public async Task Get_RelationNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserVocabSetRepo
            .Setup(r => r.GetUserVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserVocabularySet?)null);

        var act = () => service.GetUserVocabularySetAsync(1, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Get_RelationExists_ReturnsDtoWithCorrectFields()
    {
        var (service, deps) = CreateService();
        var relation = MakeRelation(userId: 1, setId: 10);
        deps.UserVocabSetRepo
            .Setup(r => r.GetUserVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(relation);

        var result = await service.GetUserVocabularySetAsync(1, 10);

        result.Should().NotBeNull();
        result.VocabularySetId.Should().Be(10);
        result.TotalCompletedSession.Should().Be(3);
        result.IsCompleted.Should().BeFalse();
        result.IsActive.Should().BeTrue();
    }

    // ============================================================================
    // RemoveVocabularySetFromUserAsync
    // ============================================================================

    [Fact]
    public async Task Remove_RelationNotFound_ReturnsFalse_NoSave()
    {
        var (service, deps) = CreateService();
        deps.UserVocabSetRepo
            .Setup(r => r.RemoveUserVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.RemoveVocabularySetFromUserAsync(1, 10);

        result.Should().BeFalse();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_RelationFound_ReturnsTrueAndSaves()
    {
        var (service, deps) = CreateService();
        deps.UserVocabSetRepo
            .Setup(r => r.RemoveUserVocabularySetAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.RemoveVocabularySetFromUserAsync(1, 10);

        result.Should().BeTrue();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
