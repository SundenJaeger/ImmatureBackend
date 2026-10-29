using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Interfaces;

public interface IImageService
{
    Task<Result<(string publicId, string secureUrl)>> UploadAsync(IFormFile file,
        CancellationToken cancellationToken = default);
}