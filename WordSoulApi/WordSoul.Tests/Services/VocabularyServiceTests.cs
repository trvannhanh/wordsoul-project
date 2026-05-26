using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services;

public class VocabularyServiceTests
{
    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<IUnitOfWork>          Uow,
        Mock<IVocabularyRepository> VocabRepo);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (VocabularyService service, Deps deps) CreateService()
    {
        var uow       = new Mock<IUnitOfWork>();
        var vocabRepo = new Mock<IVocabularyRepository>();

        uow.Setup(u => u.Vocabulary).Returns(vocabRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var service = new VocabularyService(
            uow.Object,
            new Mock<ILogger<VocabularyService>>().Object);

        return (service, new Deps(uow, vocabRepo));
    }

    // ─── Entity helper ─────────────────────────────────────────────────────────
    private static Vocabulary MakeVocab(int id = 1, string word = "hello",
        string? imageUrl = null) =>
        new Vocabulary { Id = id, Word = word, ImageUrl = imageUrl };

    // ============================================================================
    // CreateVocabularyAsync
    // ============================================================================

    [Fact]
    public async Task Create_EmptyWord_ThrowsArgumentException()
    {
        var (service, _) = CreateService();
        var dto = new CreateVocabularyDto { Word = "   ", Meaning = null };

        var act = () => service.CreateVocabularyAsync(dto, null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_ValidInput_ReturnsAdminVocabularyDto()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo
            .Setup(r => r.CreateVocabularyAsync(It.IsAny<Vocabulary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary v, CancellationToken _) => v);
        var dto = new CreateVocabularyDto { Word = " cat ", Meaning = "a pet" };

        var result = await service.CreateVocabularyAsync(dto, "http://img.test");

        result.Should().NotBeNull();
        result.Word.Should().Be("cat");           // trimmed
        result.ImageUrl.Should().Be("http://img.test");
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================================
    // GetAllVocabulariesAsync
    // ============================================================================

    [Fact]
    public async Task GetAll_ReturnsMappedList()
    {
        var (service, deps) = CreateService();
        var entities = new List<Vocabulary> { MakeVocab(1, "apple"), MakeVocab(2, "banana") };
        deps.VocabRepo
            .Setup(r => r.GetAllVocabulariesAsync(
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<PartOfSpeech?>(), It.IsAny<CEFRLevel?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var result = await service.GetAllVocabulariesAsync();

        result.Should().HaveCount(2);
        result.Select(v => v.Word).Should().Contain(["apple", "banana"]);
    }

    // ============================================================================
    // GetVocabularyByIdAsync
    // ============================================================================

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary?)null);

        var result = await service.GetVocabularyByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_Found_ReturnsMappedDto()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVocab(5, "dog"));

        var result = await service.GetVocabularyByIdAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Word.Should().Be("dog");
    }

    // ============================================================================
    // GetVocabulariesByWordsAsync
    // ============================================================================

    [Fact]
    public async Task GetByWords_NullWords_ReturnsEmpty()
    {
        var (service, _) = CreateService();
        var dto = new SearchVocabularyDto { Words = [] };

        var result = await service.GetVocabulariesByWordsAsync(dto);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByWords_WithWords_ReturnsMappedList()
    {
        var (service, deps) = CreateService();
        var entities = new List<Vocabulary> { MakeVocab(1, "cat"), MakeVocab(2, "dog") };
        deps.VocabRepo
            .Setup(r => r.GetVocabulariesByWordsAsync(
                It.IsAny<List<string>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        var dto = new SearchVocabularyDto { Words = ["dog", "cat"] };

        var result = await service.GetVocabulariesByWordsAsync(dto);

        result.Should().HaveCount(2);
    }

    // ============================================================================
    // UpdateVocabularyAsync
    // ============================================================================

    [Fact]
    public async Task Update_EmptyWord_ThrowsArgumentException()
    {
        var (service, _) = CreateService();
        var dto = new CreateVocabularyDto { Word = "", Meaning = null };

        var act = () => service.UpdateVocabularyAsync(1, dto, null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_NotFound_ThrowsKeyNotFoundException()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary?)null);
        var dto = new CreateVocabularyDto { Word = "cat", Meaning = null };

        var act = () => service.UpdateVocabularyAsync(1, dto, null);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*1*");
    }

    [Fact]
    public async Task Update_Found_UpdatesWordAndReturnsDto()
    {
        var (service, deps) = CreateService();
        var entity = MakeVocab(1, "old", imageUrl: "old.jpg");
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        deps.VocabRepo
            .Setup(r => r.UpdateVocabularyAsync(It.IsAny<Vocabulary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary v, CancellationToken _) => v);
        var dto = new CreateVocabularyDto { Word = " new ", Meaning = null };

        var result = await service.UpdateVocabularyAsync(1, dto, imageUrl: null);

        result.Should().NotBeNull();
        result!.Word.Should().Be("new");          // trimmed
        result.ImageUrl.Should().Be("old.jpg");   // kept when no new image
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NewImageUrl_OverwritesOldImage()
    {
        var (service, deps) = CreateService();
        var entity = MakeVocab(1, "word", imageUrl: "old.jpg");
        deps.VocabRepo.Setup(r => r.GetVocabularyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        deps.VocabRepo
            .Setup(r => r.UpdateVocabularyAsync(It.IsAny<Vocabulary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vocabulary v, CancellationToken _) => v);
        var dto = new CreateVocabularyDto { Word = "word", Meaning = null };

        var result = await service.UpdateVocabularyAsync(1, dto, imageUrl: "new.jpg");

        result!.ImageUrl.Should().Be("new.jpg");
    }

    // ============================================================================
    // DeleteVocabularyAsync
    // ============================================================================

    [Fact]
    public async Task Delete_RepoReturnsFalse_ReturnsFalse_NoSave()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo.Setup(r => r.DeleteVocabularyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.DeleteVocabularyAsync(1);

        result.Should().BeFalse();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_RepoReturnsTrue_ReturnsTrueAndSaves()
    {
        var (service, deps) = CreateService();
        deps.VocabRepo.Setup(r => r.DeleteVocabularyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.DeleteVocabularyAsync(1);

        result.Should().BeTrue();
        deps.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
