using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FileSignatures;
using FileSignatures.Formats;
using FluentResults;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ImmatureBackend.Application.Services;

public class CloudinaryImageService(
    Cloudinary cloudinary,
    IFileFormatInspector fileFormatInspector,
    ILogger<CloudinaryImageService> logger) : IImageService
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    public async Task<Result<(string publicId, string secureUrl)>> UploadAsync(IFormFile file,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Image upload started: {FileName}, {Size} bytes, content type {ContentType}.",
            file.FileName,
            file.Length,
            file.ContentType
        );

        if (file.Length is 0 or > MaxImageSize)
        {
            logger.LogWarning(
                "Image upload rejected: {FileName} is {Size} bytes (must be between 1 and {MaxSize}).",
                file.FileName,
                file.Length,
                MaxImageSize
            );

            return Result.Fail(new InvalidImageError("Image exceeds the maximum allowed file size."));
        }

        await using var readStream = file.OpenReadStream();
        using var memStream = new MemoryStream();

        await readStream.CopyToAsync(memStream, cancellationToken);

        memStream.Position = 0;

        var fileFormat = fileFormatInspector.DetermineFileFormat(memStream);
        if (fileFormat is not (Jpeg or Png))
        {
            logger.LogWarning(
                "Image upload rejected: {FileName} has unsupported format {DetectedFormat} (client content type {ContentType}).",
                file.FileName,
                fileFormat?.GetType().Name ?? "unknown",
                file.ContentType
            );

            return Result.Fail(new InvalidImageError());
        }

        memStream.Position = 0;

        var parameters = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, memStream),
            AssetFolder = "immature/images",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await cloudinary.UploadAsync(parameters, cancellationToken);
        if (result.Error is not null)
        {
            logger.LogError(
                "Cloudinary upload failed for {FileName} ({Size} bytes): {StatusCode} {Error}",
                file.FileName,
                file.Length,
                result.StatusCode,
                result.Error.Message
            );

            return Result.Fail(new InvalidImageError("Image upload failed."));
        }

        logger.LogInformation(
            "Image uploaded: {FileName} -> {PublicId} ({Format}, {Width}x{Height}, {Bytes} bytes).",
            file.FileName,
            result.PublicId,
            fileFormat.GetType().Name,
            result.Width,
            result.Height,
            result.Bytes
        );

        return Result.Ok((result.PublicId, result.SecureUrl.ToString()));
    }
}