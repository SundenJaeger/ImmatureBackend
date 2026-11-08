using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Exceptions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Application.Services;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ZiggyCreatures.Caching.Fusion;

namespace ImmatureBackend.Test.Application.Services;

public class ReplicateServiceTests : IDisposable
{
    private readonly Mock<IReplicateRepository> _repository = new();
    private readonly Mock<ICalculationService> _calculationService = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly Mock<ILogger<ReplicateService>> _logger = new();

    private readonly FusionCache _cache = new(new FusionCacheOptions());

    private readonly ReplicateService _service;

    public ReplicateServiceTests()
    {
        _service = new ReplicateService(
            _repository.Object,
            _calculationService.Object,
            _imageService.Object,
            _cache,
            _logger.Object);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    private static ReplicateRequest CreateRequest()
    {
        var imageBytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(imageBytes), 0, imageBytes.Length, "Image", "image.jpg");

        return new ReplicateRequest
        {
            Image = formFile,
            TechnicianName = "Tech1",
            SampleId = "S1",
            AiPredictedGrains = "[]",
            ConfirmedGrains = "[]",
            Weight = 10m,
            Season = " 2026WS ",
            IndexNumber = " 1234 ",
            SelectionName = " NSIC Rc 222 ",
            SampleNo = 2,
            CapturedAt = new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero),
            SubmittedAt = new DateTimeOffset(2026, 11, 9, 8, 5, 0, TimeSpan.Zero)
        };
    }

    private static ReplicateEntity CreateEntity(Action<ReplicateEntity>? customize = null)
    {
        var entity = new ReplicateEntity
        {
            Id = Guid.NewGuid(),
            TechnicianName = "Tech1",
            SampleId = "S1",
            ImmatureWeight = 10m,
            Percentage = 33.3m,
            Grade = Grade.NotClassified,
            AiPredictedGrains = "[]",
            ConfirmedGrains = "[]",
            ImagePublicId = "public-id",
            ImageSecureUrl = "https://cdn.example/image.jpg",
            ReviewStatus = ReviewStatus.Review,
            Season = "2026WS",
            IndexNumber = "1234",
            SelectionName = "NSIC Rc 222",
            SampleNo = 2,
            CapturedAt = new DateTime(2026, 11, 9, 8, 0, 0, DateTimeKind.Utc),
            SubmittedAt = new DateTime(2026, 11, 9, 8, 5, 0, DateTimeKind.Utc)
        };

        customize?.Invoke(entity);

        return entity;
    }

    [Fact]
    public async Task GetAllReplicateListItemsAsync_ReturnsMappedItems()
    {
        var entities = new List<ReplicateEntity>
        {
            CreateEntity(e =>
            {
                e.CreatedAt = DateTime.UtcNow;
                e.AiPredictedGrains = JsonConvert.SerializeObject(new List<GrainBox> { new() { X = 1 } });
                e.ConfirmedGrains = JsonConvert.SerializeObject(new List<GrainBox> { new() { X = 2 } });
                e.Percentage = 33.3m;
                e.Grade = Grade.NotClassified;
                e.ImagePublicId = "immature/images/abc123";
                e.ImageSecureUrl = "https://res.cloudinary.com/demo/image/upload/abc123.jpg";
            })
        };

        _calculationService.Setup(c => c.MeetsRecommendedValue(33.3m)).Returns(false);
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _service.GetAllReplicateListItemsAsync();

        result.Should().HaveCount(1);
        var item = result[0];
        item.Id.Should().Be(entities[0].Id.ToString());
        item.TechnicianName.Should().Be("Tech1");
        item.SampleId.Should().Be("S1");
        item.ImmatureWeight.Should().Be(10m);
        item.Percentage.Should().Be(33.3m);
        item.Grade.Should().Be(Grade.NotClassified);
        item.ReviewStatus.Should().Be(ReviewStatus.Review);
        item.MeetsRecommendedValue.Should().BeFalse();
        item.Season.Should().Be("2026WS");
        item.IndexNumber.Should().Be("1234");
        item.SelectionName.Should().Be("NSIC Rc 222");
        item.SampleNo.Should().Be(2);
        item.CapturedAt.Should().Be(entities[0].CapturedAt);
        item.SubmittedAt.Should().Be(entities[0].SubmittedAt);
        item.ImagePublicId.Should().Be("immature/images/abc123");
        item.ImageSecureUrl.Should().Be("https://res.cloudinary.com/demo/image/upload/abc123.jpg");
        item.AiPredictedGrains.Should().HaveCount(1);
        item.ConfirmedGrains.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateReviewStatus_ValidStatus_ReturnsUpdatedStatus()
    {
        var id = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = "Review" };
        _repository
            .Setup(r => r.UpdateStatusAsync(id, ReviewStatus.Review, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateReviewStatus(id, request);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be(id.ToString());
        result.Value.ReviewStatus.Should().Be(ReviewStatus.Review);
    }

    [Fact]
    public async Task UpdateReviewStatus_RepositoryReturnsFalse_ReturnsReplicateNotFoundError()
    {
        var id = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = "Review" };
        _repository
            .Setup(r => r.UpdateStatusAsync(id, ReviewStatus.Review, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateReviewStatus(id, request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is ReplicateNotFoundError);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsReplicateResponse()
    {
        var request = CreateRequest();

        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(33.3m);
        _calculationService.Setup(c => c.AssignGrade(33.3m)).Returns(Grade.NotClassified);
        _calculationService.Setup(c => c.MeetsRecommendedValue(33.3m)).Returns(false);

        _imageService
            .Setup(i => i.UploadAsync(request.Image, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(("public-id", "https://cdn.example/image.jpg")));

        var savedEntity = CreateEntity();
        _repository
            .Setup(r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        var result = await _service.CreateAsync(request);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be(savedEntity.Id.ToString());
        result.Value.Percentage.Should().Be(33.3m);
        result.Value.Grade.Should().Be(Grade.NotClassified);
        result.Value.MeetsRecommendedValue.Should().BeFalse();

        _repository.Verify(r => r.CreateAsync(
            It.Is<ReplicateEntity>(e =>
                e.ImagePublicId == "public-id" &&
                e.ImageSecureUrl == "https://cdn.example/image.jpg" &&
                e.Season == "2026WS" &&
                e.IndexNumber == "1234" &&
                e.SelectionName == "NSIC Rc 222" &&
                e.SampleNo == 2 &&
                e.CapturedAt == new DateTime(2026, 11, 9, 8, 0, 0, DateTimeKind.Utc) &&
                e.SubmittedAt == new DateTime(2026, 11, 9, 8, 5, 0, DateTimeKind.Utc)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PercentageBelowTwo_ReportsMeetsRecommendedValue()
    {
        var request = CreateRequest();

        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(1.9m);
        _calculationService.Setup(c => c.AssignGrade(1.9m)).Returns(Grade.Pr);
        _calculationService.Setup(c => c.MeetsRecommendedValue(1.9m)).Returns(true);
        _imageService
            .Setup(i => i.UploadAsync(request.Image, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(("public-id", "https://cdn.example/image.jpg")));
        _repository
            .Setup(r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEntity(e =>
            {
                e.Percentage = 1.9m;
                e.Grade = Grade.Pr;
                e.MeetsRecommendedValue = true;
            }));

        var result = await _service.CreateAsync(request);

        result.Should().BeSuccess();
        result.Value.MeetsRecommendedValue.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SampleAlreadyExists_ReturnsDuplicateSampleErrorWithoutUploadingOrSaving()
    {
        var request = CreateRequest();

        _repository
            .Setup(r => r.ExistsAsync("2026WS", "1234", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is DuplicateSampleError);
        _imageService.Verify(
            i => i.UploadAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repository.Verify(
            r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UniqueIndexViolationOnSave_ReturnsDuplicateSampleError()
    {
        var request = CreateRequest();

        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(33.3m);
        _calculationService.Setup(c => c.AssignGrade(33.3m)).Returns(Grade.NotClassified);
        _imageService
            .Setup(i => i.UploadAsync(request.Image, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(("public-id", "https://cdn.example/image.jpg")));
        _repository
            .Setup(r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateReplicateException(new Exception("23505")));

        var result = await _service.CreateAsync(request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is DuplicateSampleError);
    }

    [Fact]
    public async Task CreateAsync_ImageUploadFails_ReturnsFailureAndDoesNotSave()
    {
        var request = CreateRequest();

        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(33.3m);
        _calculationService.Setup(c => c.AssignGrade(33.3m)).Returns(Grade.NotClassified);

        _imageService
            .Setup(i => i.UploadAsync(request.Image, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<(string, string)>(new InvalidImageError()));

        var result = await _service.CreateAsync(request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is InvalidImageError);
        _repository.Verify(
            r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllReplicateListItemsAsync_CalledTwice_HitsRepositoryOnce()
    {
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReplicateEntity>());

        await _service.GetAllReplicateListItemsAsync();
        await _service.GetAllReplicateListItemsAsync();

        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewStatus_Success_InvalidatesListCache()
    {
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReplicateEntity>());
        _repository
            .Setup(r => r.UpdateStatusAsync(id, ReviewStatus.Review, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.GetAllReplicateListItemsAsync();
        await _service.UpdateReviewStatus(id, new UpdateStatusRequest { Status = "Review" });
        await _service.GetAllReplicateListItemsAsync();

        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateAsync_Success_InvalidatesListCache()
    {
        var request = CreateRequest();
        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(33.3m);
        _calculationService.Setup(c => c.AssignGrade(33.3m)).Returns(Grade.NotClassified);
        _imageService
            .Setup(i => i.UploadAsync(request.Image, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(("public-id", "https://cdn.example/image.jpg")));
        _repository
            .Setup(r => r.CreateAsync(It.IsAny<ReplicateEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEntity());
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReplicateEntity>());

        await _service.GetAllReplicateListItemsAsync();
        await _service.CreateAsync(request);
        await _service.GetAllReplicateListItemsAsync();

        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}