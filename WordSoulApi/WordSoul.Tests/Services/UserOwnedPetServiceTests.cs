using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services;

public class UserOwnedPetServiceTests
{
    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<IUnitOfWork>             Uow,
        Mock<IPetRepository>          PetRepo,
        Mock<IUserRepository>         UserRepo,
        Mock<IUserOwnedPetRepository> UserOwnedPetRepo,
        Mock<IActivityLogService>     ActivityLog,
        Mock<IDailyQuestService>      DailyQuest);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (UserOwnedPetService service, Deps deps) CreateService()
    {
        var uow              = new Mock<IUnitOfWork>();
        var petRepo          = new Mock<IPetRepository>();
        var userRepo         = new Mock<IUserRepository>();
        var userOwnedPetRepo = new Mock<IUserOwnedPetRepository>();
        var activityLog      = new Mock<IActivityLogService>();
        var dailyQuest       = new Mock<IDailyQuestService>();

        uow.Setup(u => u.Pet).Returns(petRepo.Object);
        uow.Setup(u => u.User).Returns(userRepo.Object);
        uow.Setup(u => u.UserOwnedPet).Returns(userOwnedPetRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        activityLog
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        dailyQuest
            .Setup(d => d.UpdateQuestProgressAsync(
                It.IsAny<int>(), It.IsAny<QuestType>(), It.IsAny<int>(),
                It.IsAny<double?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserOwnedPetService(
            uow.Object, activityLog.Object, dailyQuest.Object,
            new Mock<ILogger<UserOwnedPetService>>().Object);

        return (service, new Deps(uow, petRepo, userRepo, userOwnedPetRepo, activityLog, dailyQuest));
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────
    private static Pet MakePet(int id = 1, string name = "Pikachu") =>
        new Pet { Id = id, Name = name };

    private static User MakeUser(int id = 1) =>
        new User { Id = id, Email = $"u{id}@test.com" };

    private static UserOwnedPet MakeOwnedPet(int userId = 1, int petId = 1, int level = 1, int xp = 0) =>
        new UserOwnedPet { UserId = userId, PetId = petId, Level = level, Experience = xp };

    // ──────────────────────────────────────────────────────────────────────────
    // AssignPetToUserAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Assign_PetNotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);
        deps.UserRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());

        var result = await service.AssignPetToUserAsync(new AssignPetDto { PetId = 99, UserId = 1 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Assign_UserNotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet());
        deps.UserRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await service.AssignPetToUserAsync(new AssignPetDto { PetId = 1, UserId = 99 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Assign_PetAlreadyOwned_ThrowsArgumentException()
    {
        var (service, deps) = CreateService();
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet());
        deps.UserRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => service.AssignPetToUserAsync(new AssignPetDto { PetId = 1, UserId = 1 });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Assign_Success_ReturnsDtoWithCorrectFields()
    {
        var (service, deps) = CreateService();
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet(id: 5, name: "Charmander"));
        deps.UserRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(id: 2));
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.UserOwnedPetRepo
            .Setup(r => r.CreateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.AssignPetToUserAsync(
            new AssignPetDto { PetId = 5, UserId = 2, InitialLevel = 3, IsActive = true });

        result.Should().NotBeNull();
        result!.PetId.Should().Be(5);
        result.UserId.Should().Be(2);
        result.Level.Should().Be(3);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GrantPetAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Grant_AlreadyOwned_ReturnsBonusXp50()
    {
        var (service, deps) = CreateService();
        var user = MakeUser();
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        deps.UserRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        deps.UserRepo
            .Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var (alreadyOwned, isSuccess, bonusXp) = await service.GrantPetAsync(1, 10, 0.8);

        alreadyOwned.Should().BeTrue();
        isSuccess.Should().BeFalse();
        bonusXp.Should().Be(50);
    }

    [Fact]
    public async Task Grant_CatchFails_ReturnsFail()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // catchRate=-1.0 → random.NextDouble() <= -1.0 is always false
        var (alreadyOwned, isSuccess, bonusXp) = await service.GrantPetAsync(1, 10, catchRate: -1.0);

        alreadyOwned.Should().BeFalse();
        isSuccess.Should().BeFalse();
        bonusXp.Should().Be(0);
    }

    [Fact]
    public async Task Grant_CatchSucceeds_NoActivePet_NewPetSetToActive()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.UserOwnedPetRepo
            .Setup(r => r.GetActivePetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);
        deps.UserOwnedPetRepo
            .Setup(r => r.CreateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet());

        // catchRate=2.0 → random.NextDouble() <= 2.0 is always true
        var (alreadyOwned, isSuccess, bonusXp) = await service.GrantPetAsync(1, 10, catchRate: 2.0);

        alreadyOwned.Should().BeFalse();
        isSuccess.Should().BeTrue();
        bonusXp.Should().Be(0);
        deps.UserOwnedPetRepo.Verify(
            r => r.CreateUserOwnedPetAsync(
                It.Is<UserOwnedPet>(p => p.IsActive == true), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Grant_CatchSucceeds_HasActivePet_NewPetSetToInactive()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.CheckPetOwnedByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.UserOwnedPetRepo
            .Setup(r => r.GetActivePetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet(id: 99));   // user already has an active pet
        deps.UserOwnedPetRepo
            .Setup(r => r.CreateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet());

        var (_, isSuccess, _) = await service.GrantPetAsync(1, 10, catchRate: 2.0);

        isSuccess.Should().BeTrue();
        deps.UserOwnedPetRepo.Verify(
            r => r.CreateUserOwnedPetAsync(
                It.Is<UserOwnedPet>(p => p.IsActive == false), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UpgradePetForUserAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Upgrade_PetNotOwned_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOwnedPet?)null);

        var act = () => service.UpgradePetForUserAsync(1, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Upgrade_XPBelowThreshold_NoLevelUp()
    {
        var (service, deps) = CreateService();
        var owned = MakeOwnedPet(xp: 0, level: 1);
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(owned);
        deps.UserOwnedPetRepo
            .Setup(r => r.UpdateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.UpgradePetForUserAsync(1, 1, experience: 50);

        result.Should().NotBeNull();
        result!.IsLevelUp.Should().BeFalse();
        result.IsEvolved.Should().BeFalse();
        result.Experience.Should().Be(50);
        result.Level.Should().Be(1);
    }

    [Fact]
    public async Task Upgrade_XPReaches100_LevelUp_NoEvolution()
    {
        var (service, deps) = CreateService();
        var owned = MakeOwnedPet(xp: 80, level: 1);   // 80+30=110 → level up, XP=10
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(owned);
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pet { Id = 1, Name = "Pikachu", NextEvolutionId = null });   // no evolution
        deps.UserOwnedPetRepo
            .Setup(r => r.UpdateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.UpgradePetForUserAsync(1, 1, experience: 30);

        result.Should().NotBeNull();
        result!.IsLevelUp.Should().BeTrue();
        result.IsEvolved.Should().BeFalse();
        result.Level.Should().Be(2);
        result.Experience.Should().Be(10);
    }

    [Fact]
    public async Task Upgrade_LevelUp_EvolutionTriggered()
    {
        var (service, deps) = CreateService();
        // Pet at level=1, XP=90; add 20 → XP=110 → level up (level=2, XP=10)
        // currentPet has NextEvolutionId=2, RequiredLevel=2 → evolves to pet 2
        var owned = MakeOwnedPet(xp: 90, level: 1);
        var currentPet = new Pet { Id = 1, Name = "Bulbasaur", NextEvolutionId = 2, RequiredLevel = 2 };
        var evolvedPet = new Pet { Id = 2, Name = "Ivysaur" };

        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(owned);
        // GetPetByIdAsync(1) → currentPet; GetPetByIdAsync(2) → evolvedPet
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPet);
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evolvedPet);
        deps.UserOwnedPetRepo
            .Setup(r => r.UpdateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.UpgradePetForUserAsync(1, 1, experience: 20);

        result.Should().NotBeNull();
        result!.IsLevelUp.Should().BeTrue();
        result.IsEvolved.Should().BeTrue();
        result.PetId.Should().Be(2);   // evolved to Ivysaur
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ActivePetAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Active_PetNotOwned_ThrowsInvalidOperationException()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOwnedPet?)null);

        var act = () => service.ActivePetAsync(1, 10);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Active_Success_DeactivatesOthers_ActivatesTarget()
    {
        var (service, deps) = CreateService();
        var target    = MakeOwnedPet(userId: 1, petId: 5);
        var otherPet  = MakeOwnedPet(userId: 1, petId: 3);
        otherPet.IsActive = true;

        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);
        deps.UserOwnedPetRepo
            .Setup(r => r.GetAllUserOwnedPetByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOwnedPet> { otherPet, target }.AsEnumerable());
        deps.UserOwnedPetRepo
            .Setup(r => r.UpdateUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        deps.PetRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePet(id: 5, name: "Eevee"));

        var result = await service.ActivePetAsync(1, 5);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
        result.Name.Should().Be("Eevee");
        otherPet.IsActive.Should().BeFalse();   // deactivated by service
        target.IsActive.Should().BeTrue();       // activated by service
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RemovePetFromUserAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_PetNotOwned_ReturnsFalse()
    {
        var (service, deps) = CreateService();
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOwnedPet?)null);

        var result = await service.RemovePetFromUserAsync(1, 99);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Remove_PetOwned_DeletesAndReturnsTrue()
    {
        var (service, deps) = CreateService();
        var owned = MakeOwnedPet();
        deps.UserOwnedPetRepo
            .Setup(r => r.GetUserOwnedPetByUserAndPetIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(owned);
        deps.UserOwnedPetRepo
            .Setup(r => r.DeleteUserOwnedPetAsync(It.IsAny<UserOwnedPet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.RemovePetFromUserAsync(1, 1);

        result.Should().BeTrue();
        deps.UserOwnedPetRepo.Verify(
            r => r.DeleteUserOwnedPetAsync(owned, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
