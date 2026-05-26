using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services;

public class VocabularySetServiceTests
{
    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<IUnitOfWork>                    Uow,
        Mock<IVocabularySetRepository>       VocabSetRepo,
        Mock<IVocabularyRepository>          VocabRepo,
        Mock<IUserRepository>                UserRepo,
        Mock<IPetRepository>                 PetRepo,
        Mock<IUserVocabularySetRepository>   UserVocabSetRepo,
        Mock<ISystemConfigurationRepository> SysConfigRepo,
        Mock<IGeminiAiService>               Gemini,
        Mock<IAzureSpeechService>            Speech,
        Mock<IUnsplashService>               Unsplash,
        Mock<IVocabularyAiCacheService>      AiCache);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (VocabularySetService service, Deps deps) CreateService()
    {
        var uow              = new Mock<IUnitOfWork>();
        var vocabSetRepo     = new Mock<IVocabularySetRepository>();
        var vocabRepo        = new Mock<IVocabularyRepository>();
        var userRepo         = new Mock<IUserRepository>();
        var petRepo          = new Mock<IPetRepository>();
        var userVocabSetRepo = new Mock<IUserVocabularySetRepository>();
        var sysConfigRepo    = new Mock<ISystemConfigurationRepository>();
        var gemini           = new Mock<IGeminiAiService>();
        var speech           = new Mock<IAzureSpeechService>();
        var unsplash         = new Mock<IUnsplashService>();
        var aiCache          = new Mock<IVocabularyAiCacheService>();

        uow.Setup(u => u.VocabularySet).Returns(vocabSetRepo.Object);
        uow.Setup(u => u.Vocabulary).Returns(vocabRepo.Object);
        uow.Setup(u => u.User).Returns(userRepo.Object);
        uow.Setup(u => u.Pet).Returns(petRepo.Object);
        uow.Setup(u => u.UserVocabularySet).Returns(userVocabSetRepo.Object);
        uow.Setup(u => u.SystemConfiguration).Returns(sysConfigRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var service = new VocabularySetService(
            uow.Object,
            new Mock<ILogger<VocabularySetService>>().Object,
            gemini.Object,
            speech.Object,
            unsplash.Object,
            aiCache.Object);

        return (service, new Deps(uow, vocabSetRepo, vocabRepo, userRepo, petRepo,
            userVocabSetRepo, sysConfigRepo, gemini, speech, unsplash, aiCache));
    }

    // ─── Entity helpers ────────────────────────────────────────────────────────
    private static VocabularySet MakeSet(int id = 1, string title = "Test Set",
        int? createdById = null, bool isPublic = false) =>
        new VocabularySet { Id = id, Title = title, CreatedById = createdById, IsPublic = isPublic };

    private static Vocabulary MakeVocab(int id = 1, string word = "hello", int? creatorId = null) =>
        new Vocabulary { Id = id, Word = word, CreatorId = creatorId };

    private static User MakeUser(int id = 1) =>
        new User { Id = id, Email = $"u{id}@test.com" };

    // ─── Setup helper: configure pet repo to always return enough pets ─────────
    private static void SetupPetRepo(Mock<IPetRepository> petRepo) =>
        petRepo.Setup(r => r.GetRandomPetsByRarityAsync(
                It.IsAny<PetRarity>(), It.IsAny<int>(),
                It.IsAny<IEnumerable<PetType>?>(), It.IsAny<CancellationToken>()))
            .Returns<PetRarity, int, IEnumerable<PetType>?, CancellationToken>((_, count, _, _) =>
                Task.FromResult(
                    Enumerable.Range(1, count)
                        .Select(i => new Pet { Id = i, Name = $"Pet{i}" })
                        .ToList()));

    // ============================================================================
    // CreateVocabularySetAsync
    // ============================================================================

    [Fact]
    public async Task Create_EmptyTitle_ThrowsArgumentException()
    {
        var (service, _) = CreateService();
        var dto = new CreateVocabularySetDto { Title = "   ", VocabularyIds = [] };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_UserNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var dto = new CreateVocabularySetDto { Title = "My Set", VocabularyIds = [1] };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User 1*");
    }

    [Fact]
    public async Task Create_DuplicateVocabIds_ThrowsArgumentException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        var dto = new CreateVocabularySetDto { Title = "My Set", VocabularyIds = [1, 2, 1] };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*unique*");
    }

    [Fact]
    public async Task Create_TooManyVocabs_ThrowsArgumentException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        var dto = new CreateVocabularySetDto
        {
            Title = "My Set",
            VocabularyIds = Enumerable.Range(1, 51).ToList()
        };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*50*");
    }

    [Fact]
    public async Task Create_VocabNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary?)null);
        var dto = new CreateVocabularySetDto { Title = "My Set", VocabularyIds = [99] };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task Create_NotEnoughPets_ThrowsInvalidOperationException()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVocab(1));
        // Pet repo always returns empty list → not enough pets
        deps.PetRepo.Setup(r => r.GetRandomPetsByRarityAsync(
                It.IsAny<PetRarity>(), It.IsAny<int>(),
                It.IsAny<IEnumerable<PetType>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var dto = new CreateVocabularySetDto { Title = "My Set", VocabularyIds = [1] };

        var act = () => service.CreateVocabularySetAsync(dto, null, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Not enough*");
    }

    [Fact]
    public async Task Create_ValidInput_ReturnsVocabularySetDto()
    {
        var (service, deps) = CreateService();
        deps.UserRepo.Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(1));
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVocab(1));
        SetupPetRepo(deps.PetRepo);
        deps.VocabSetRepo
            .Setup(r => r.CreateVocabularySetAsync(It.IsAny<VocabularySet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet s, CancellationToken _) => s);
        deps.UserVocabSetRepo
            .Setup(r => r.AddVocabularySetToUserAsync(It.IsAny<UserVocabularySet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var dto = new CreateVocabularySetDto { Title = "My Set", VocabularyIds = [1] };

        var result = await service.CreateVocabularySetAsync(dto, null, 1);

        result.Should().NotBeNull();
        result.Title.Should().Be("My Set");
    }

    // ============================================================================
    // GetVocabularySetByIdAsync
    // ============================================================================

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);

        var result = await service.GetVocabularySetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_Found_ReturnsDetailDto()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(id: 5, title: "My Vocab Set");
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);

        var result = await service.GetVocabularySetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Title.Should().Be("My Vocab Set");
    }

    // ============================================================================
    // GetAllVocabularySetsAsync
    // ============================================================================

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        var (service, deps) = CreateService();
        var sets = new List<VocabularySet> { MakeSet(id: 1, title: "Set A"), MakeSet(id: 2, title: "Set B") };
        deps.VocabSetRepo
            .Setup(r => r.GetAllVocabularySetsAsync(
                It.IsAny<string?>(), It.IsAny<VocabularySetTheme?>(), It.IsAny<VocabularyDifficultyLevel?>(),
                It.IsAny<DateTime?>(), It.IsAny<bool?>(), It.IsAny<int?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sets);

        var result = await service.GetAllVocabularySetsAsync();

        result.Should().HaveCount(2);
        result.Select(s => s.Title).Should().Contain(["Set A", "Set B"]);
    }

    // ============================================================================
    // UpdateVocabularySetAsync
    // ============================================================================

    [Fact]
    public async Task Update_EmptyTitle_ThrowsArgumentException()
    {
        var (service, _) = CreateService();
        var dto = new UpdateVocabularySetDto { Title = "" };

        var act = () => service.UpdateVocabularySetAsync(1, dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_SetNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);
        var dto = new UpdateVocabularySetDto { Title = "New Title" };

        var act = () => service.UpdateVocabularySetAsync(1, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 10); // owned by user 10
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var dto = new UpdateVocabularySetDto { Title = "New Title" };

        // requesting as user 99 (not owner)
        var act = () => service.UpdateVocabularySetAsync(1, dto, requestingUserId: 99);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Update_AdminNullUserId_UpdatesRegardlessOfOwner()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 10); // owned by user 10
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        deps.VocabSetRepo
            .Setup(r => r.UpdateVocabularySetAsync(It.IsAny<VocabularySet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var dto = new UpdateVocabularySetDto { Title = "Admin Updated" };

        // requestingUserId = null → admin bypass
        var result = await service.UpdateVocabularySetAsync(1, dto, requestingUserId: null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_Owner_Success_ReturnsDto()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(id: 1, title: "Old Title", createdById: 5);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        deps.VocabSetRepo
            .Setup(r => r.UpdateVocabularySetAsync(It.IsAny<VocabularySet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var dto = new UpdateVocabularySetDto { Title = "New Title" };

        var result = await service.UpdateVocabularySetAsync(1, dto, requestingUserId: 5);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
    }

    // ============================================================================
    // DeleteVocabularySetAsync
    // ============================================================================

    [Fact]
    public async Task Delete_SetNotFoundWithUserId_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);

        var act = () => service.DeleteVocabularySetAsync(1, requestingUserId: 5);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 10);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);

        var act = () => service.DeleteVocabularySetAsync(1, requestingUserId: 99);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_AdminNullUserId_DeletesWithoutOwnerCheck()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.DeleteVocabularySetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.DeleteVocabularySetAsync(1, requestingUserId: null);

        result.Should().BeTrue();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_RepoReturnsFalse_ReturnsFalse()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.DeleteVocabularySetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.DeleteVocabularySetAsync(1, requestingUserId: null);

        result.Should().BeFalse();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ============================================================================
    // PublishVocabularySetAsync
    // ============================================================================

    [Fact]
    public async Task Publish_SetNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);

        var act = () => service.PublishVocabularySetAsync(1, requestingUserId: 5);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Publish_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 10, isPublic: false);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);

        var act = () => service.PublishVocabularySetAsync(1, requestingUserId: 99);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Publish_AlreadyPublic_ThrowsInvalidOperationException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 5, isPublic: true);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);

        var act = () => service.PublishVocabularySetAsync(1, requestingUserId: 5);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Publish_Success_SetsIsPublicAndReturnsDto()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(id: 1, createdById: 5, isPublic: false);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        deps.VocabSetRepo
            .Setup(r => r.UpdateVocabularySetAsync(It.IsAny<VocabularySet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);

        var result = await service.PublishVocabularySetAsync(1, requestingUserId: 5);

        result.Should().NotBeNull();
        set.IsPublic.Should().BeTrue();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // UpdateVocabularyCoreAsync
    // ============================================================================

    [Fact]
    public async Task UpdateCore_SetNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VocabularySet?)null);
        var dto = new UpdateVocabularyCoreDto { Word = "cat" };

        var act = () => service.UpdateVocabularyCoreAsync(1, 1, dto, userId: 5);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateCore_NotSetOwner_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 10);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var dto = new UpdateVocabularyCoreDto { Word = "cat" };

        var act = () => service.UpdateVocabularyCoreAsync(1, 1, dto, userId: 99);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateCore_VocabNotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 5);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary?)null);
        var dto = new UpdateVocabularyCoreDto { Word = "cat" };

        var act = () => service.UpdateVocabularyCoreAsync(1, 99, dto, userId: 5);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateCore_NotVocabCreator_ThrowsUnauthorizedAccessException()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 5);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var vocab = MakeVocab(id: 1, creatorId: 99); // owned by user 99
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vocab);
        var dto = new UpdateVocabularyCoreDto { Word = "cat" };

        var act = () => service.UpdateVocabularyCoreAsync(1, 1, dto, userId: 5);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateCore_Success_ReturnsAdminVocabularyDto()
    {
        var (service, deps) = CreateService();
        var set = MakeSet(createdById: 5);
        deps.VocabSetRepo.Setup(r => r.GetVocabularySetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(set);
        var vocab = MakeVocab(id: 1, word: "hello", creatorId: 5);
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vocab);
        deps.VocabRepo
            .Setup(r => r.UpdateVocabularyAsync(It.IsAny<Vocabulary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary v, CancellationToken _) => v);
        var dto = new UpdateVocabularyCoreDto { Word = "world" };

        var result = await service.UpdateVocabularyCoreAsync(1, 1, dto, userId: 5);

        result.Should().NotBeNull();
        result!.Word.Should().Be("world");
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
