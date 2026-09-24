using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Application.Services;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using FileSignatures;
using FileSignatures.Formats;
using Microsoft.AspNetCore.Http.Internal;

namespace ImmatureBackend.Test.Application.Services;

public class ReplicateServiceTests
{
    private readonly Mock<IReplicateRepository> _repository = new();
    private readonly Mock<ICalculationService> _calculationService = new();
    private readonly Mock<IFileFormatInspector> _fileFormatInspector = new();
    private readonly Mock<ILogger<ReplicateService>> _logger = new();

    private readonly ReplicateService _service;

    public ReplicateServiceTests()
    {
        _service = new ReplicateService(
            _repository.Object,
            _calculationService.Object,
            _fileFormatInspector.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllReplicateListItemsAsync_ReturnsMappedItems()
    {
        var entities = new List<ReplicateEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TechnicianName = "Tech1",
                CreatedAt = DateTime.UtcNow,
                SampleId = "S1",
                AiPredictedGrains = JsonConvert.SerializeObject(new List<GrainBox> { new() { X = 1 } }),
                ConfirmedGrains = JsonConvert.SerializeObject(new List<GrainBox> { new() { X = 2 } }),
                ImmatureWeight = 10m,
                Percentage = 33.33m,
                Grade = Grade.G1,
                ReviewStatus = ReviewStatus.Review
            }
        };

        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        var result = await _service.GetAllReplicateListItemsAsync();

        result.Should().HaveCount(1);
        var item = result[0];
        item.Id.Should().Be(entities[0].Id.ToString());
        item.TechnicianName.Should().Be("Tech1");
        item.SampleId.Should().Be("S1");
        item.ImmatureWeight.Should().Be(10m);
        item.Percentage.Should().Be(33.33m);
        item.Grade.Should().Be(Grade.G1);
        item.ReviewStatus.Should().Be(ReviewStatus.Review);
        item.AiPredictedGrains.Should().HaveCount(1);
        item.ConfirmedGrains.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetImage_ValidImage_ReturnsBytesAndContentType()
    {
        var id = Guid.NewGuid();
        var imageBytes = new byte[] { 1, 2, 3 };
        _repository.Setup(r => r.GetImageBytesAsync(id)).ReturnsAsync(imageBytes);

        _fileFormatInspector
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new Jpeg());

        var result = await _service.GetImage(id);

        result.Should().BeSuccess();
        result.Value.bytes.Should().BeEquivalentTo(imageBytes);
        result.Value.contentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task GetImage_NullOrEmptyBytes_ReturnsImageNotFoundError()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetImageBytesAsync(id)).ReturnsAsync((byte[]?)null);

        var result = await _service.GetImage(id);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is ImageNotFoundError);
    }

    [Fact]
    public async Task GetImage_UnsupportedFormat_ReturnsInvalidImageError()
    {
        var id = Guid.NewGuid();
        var imageBytes = new byte[] { 1, 2, 3 };
        _repository.Setup(r => r.GetImageBytesAsync(id)).ReturnsAsync(imageBytes);

        _fileFormatInspector
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new Pdf());

        var result = await _service.GetImage(id);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is InvalidImageError);
    }

    [Fact]
    public async Task UpdateReviewStatus_ValidStatus_ReturnsUpdatedStatus()
    {
        var id = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = "Review" };
        _repository.Setup(r => r.UpdateStatusAsync(id, ReviewStatus.Review)).ReturnsAsync(true);

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
        _repository.Setup(r => r.UpdateStatusAsync(id, ReviewStatus.Review)).ReturnsAsync(false);

        var result = await _service.UpdateReviewStatus(id, request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is ReplicateNotFoundError);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsReplicateResponse()
    {
        var imageBytes = new byte[] { 1, 2, 3 };
        var imageStream = new MemoryStream(imageBytes);
        var formFile = new FormFile(imageStream, 0, imageBytes.Length, "Image", "image.jpg");

        var request = new ReplicateRequest
        {
            Image = formFile,
            TechnicianName = "Tech1",
            SampleId = "S1",
            AiPredictedGrains = "[]",
            ConfirmedGrains = "[]",
            Weight = 10m
        };

        _calculationService.Setup(c => c.CalculatePercentage(10m)).Returns(33.33m);
        _calculationService.Setup(c => c.AssignGrade(33.33m)).Returns(Grade.G1);

        _fileFormatInspector
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new Jpeg());

        var savedEntity = new ReplicateEntity
        {
            Id = Guid.NewGuid(),
            TechnicianName = "Tech1",
            SampleId = "S1",
            ImmatureWeight = 10m,
            Percentage = 33.33m,
            Grade = Grade.G1,
            AiPredictedGrains = "[]",
            ConfirmedGrains = "[]",
            ReviewStatus = ReviewStatus.Review
        };
        _repository.Setup(r => r.CreateAsync(It.IsAny<ReplicateEntity>())).ReturnsAsync(savedEntity);

        var result = await _service.CreateAsync(request);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be(savedEntity.Id.ToString());
        result.Value.Percentage.Should().Be(33.33m);
        result.Value.Grade.Should().Be(Grade.G1);
    }

    [Fact]
    public async Task CreateAsync_UnsupportedImageFormat_ReturnsInvalidImageError()
    {
        var imageBytes = new byte[] { 1, 2, 3 };
        var imageStream = new MemoryStream(imageBytes);
        var formFile = new FormFile(imageStream, 0, imageBytes.Length, "Image", "image.txt");

        var request = new ReplicateRequest
        {
            Image = formFile,
            TechnicianName = "Tech1",
            SampleId = "S1",
            AiPredictedGrains = "[]",
            ConfirmedGrains = "[]",
            Weight = 10m
        };

        _fileFormatInspector
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new Pdf());

        var result = await _service.CreateAsync(request);

        result.Should().BeFailure();
        result.Errors.Should().ContainSingle(e => e is InvalidImageError);
    }
}