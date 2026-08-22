using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services;
using WordSoul.Application.Services.SRS;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services;

public sealed class SystemConfigurationServiceTests
{
    [Fact]
    public async Task UpdateConfigurationsAsync_SrsChange_IncrementsPolicyVersion()
    {
        var configurations = new List<SystemConfiguration>
        {
            Create(
                SrsAlgorithmSettings.PolicyVersionKey,
                "1",
                "Integer",
                isLiveEditable: false),
            Create(
                SrsAlgorithmSettings.FirstIntervalKey,
                "1",
                "Integer",
                minValue: 0,
                maxValue: 30)
        };
        var (service, repository, unitOfWork) =
            CreateService(configurations);

        await service.UpdateConfigurationsAsync(
            [
                new UpdateSystemConfigurationDto(
                    SrsAlgorithmSettings.PolicyVersionKey,
                    "1"),
                new UpdateSystemConfigurationDto(
                    SrsAlgorithmSettings.FirstIntervalKey,
                    "2")
            ],
            "admin");

        configurations.Single(item =>
                item.Key == SrsAlgorithmSettings.FirstIntervalKey)
            .Value.Should().Be("2");
        var version = configurations.Single(item =>
            item.Key == SrsAlgorithmSettings.PolicyVersionKey);
        version.Value.Should().Be("2");
        version.LastUpdatedBy.Should().Be("admin");
        repository.Verify(repo => repo.UpdateBulkAsync(
            It.Is<IEnumerable<SystemConfiguration>>(items =>
                items.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigurationsAsync_ValueOutsideBounds_IsRejected()
    {
        var configurations = new List<SystemConfiguration>
        {
            Create(
                "WordsPerSession",
                "5",
                "Integer",
                minValue: 1,
                maxValue: 30)
        };
        var (service, repository, unitOfWork) =
            CreateService(configurations);

        var act = () => service.UpdateConfigurationsAsync(
            [new UpdateSystemConfigurationDto("WordsPerSession", "31")],
            "admin");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        repository.Verify(repo => repo.UpdateBulkAsync(
            It.IsAny<IEnumerable<SystemConfiguration>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateConfigurationsAsync_ManagedValueChange_IsRejected()
    {
        var configurations = new List<SystemConfiguration>
        {
            Create(
                SrsAlgorithmSettings.PolicyVersionKey,
                "1",
                "Integer",
                isLiveEditable: false)
        };
        var (service, repository, _) = CreateService(configurations);

        var act = () => service.UpdateConfigurationsAsync(
            [
                new UpdateSystemConfigurationDto(
                    SrsAlgorithmSettings.PolicyVersionKey,
                    "2")
            ],
            "admin");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not live-editable*");
        repository.Verify(repo => repo.UpdateBulkAsync(
            It.IsAny<IEnumerable<SystemConfiguration>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateConfigurationsAsync_InvalidSrsCombination_IsRejected()
    {
        var configurations = new List<SystemConfiguration>
        {
            Create(
                SrsAlgorithmSettings.PolicyVersionKey,
                "1",
                "Integer",
                isLiveEditable: false),
            Create(
                SrsAlgorithmSettings.MaxEaseFactorKey,
                "4",
                "Float",
                minValue: 1.3,
                maxValue: 5)
        };
        var (service, repository, _) = CreateService(configurations);

        var act = () => service.UpdateConfigurationsAsync(
            [
                new UpdateSystemConfigurationDto(
                    SrsAlgorithmSettings.MaxEaseFactorKey,
                    "2")
            ],
            "admin");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*default ease factor*");
        repository.Verify(repo => repo.UpdateBulkAsync(
            It.IsAny<IEnumerable<SystemConfiguration>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateConfigurationsAsync_EquivalentNumericValue_DoesNotBumpVersion()
    {
        var configurations = new List<SystemConfiguration>
        {
            Create(
                SrsAlgorithmSettings.PolicyVersionKey,
                "3",
                "Integer",
                isLiveEditable: false),
            Create(
                SrsAlgorithmSettings.MaxEaseFactorKey,
                "4.0",
                "Float",
                minValue: 1.3,
                maxValue: 5)
        };
        var (service, repository, unitOfWork) =
            CreateService(configurations);

        await service.UpdateConfigurationsAsync(
            [
                new UpdateSystemConfigurationDto(
                    SrsAlgorithmSettings.MaxEaseFactorKey,
                    "4")
            ],
            "admin");

        configurations.Single(item =>
                item.Key == SrsAlgorithmSettings.PolicyVersionKey)
            .Value.Should().Be("3");
        repository.Verify(repo => repo.UpdateBulkAsync(
            It.IsAny<IEnumerable<SystemConfiguration>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (
        SystemConfigurationService Service,
        Mock<ISystemConfigurationRepository> Repository,
        Mock<IUnitOfWork> UnitOfWork)
        CreateService(List<SystemConfiguration> configurations)
    {
        var repository = new Mock<ISystemConfigurationRepository>();
        repository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurations);
        repository
            .Setup(repo => repo.UpdateBulkAsync(
                It.IsAny<IEnumerable<SystemConfiguration>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(unit => unit.SystemConfiguration)
            .Returns(repository.Object);
        unitOfWork
            .Setup(unit => unit.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return (
            new SystemConfigurationService(
                unitOfWork.Object,
                NullLogger<SystemConfigurationService>.Instance),
            repository,
            unitOfWork);
    }

    private static SystemConfiguration Create(
        string key,
        string value,
        string dataType,
        double? minValue = null,
        double? maxValue = null,
        bool isLiveEditable = true)
    {
        return new SystemConfiguration
        {
            Key = key,
            Value = value,
            DataType = dataType,
            Category = key.StartsWith("Srs", StringComparison.Ordinal)
                ? "SRS"
                : "LEARNING",
            MinValue = minValue,
            MaxValue = maxValue,
            IsLiveEditable = isLiveEditable
        };
    }
}
