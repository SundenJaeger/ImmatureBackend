using FluentResults;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Responses;
using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.IntegrationTest.Infrastructure;

public sealed class FakeImageService : IImageService
{
    public const string FailingFileName = "fail.png";

    public Task<Result<(string publicId, string secureUrl)>> UploadAsync(IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.FileName == FailingFileName)
        {
            return Task.FromResult(
                Result.Fail<(string publicId, string secureUrl)>(new InvalidImageError("Image upload failed.")));
        }

        var publicId = $"test/{Guid.NewGuid():N}";

        return Task.FromResult(Result.Ok((publicId, secureUrl: $"https://images.test.local/{publicId}.png")));
    }
}

public sealed class FakeGrainDetector : IGrainDetector
{
    public PredictResponse Detect(byte[] imageBytes)
    {
        return new PredictResponse
        {
            ImageId = "fake-image",
            Grains = [new GrainBox { X = 10, Y = 20, Width = 30, Height = 40, Confidence = 0.92 }]
        };
    }
}