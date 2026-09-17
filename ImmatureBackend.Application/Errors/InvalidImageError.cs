using FluentResults;

namespace ImmatureBackend.Application.Errors;

public sealed class InvalidImageError() : Error("Image can only be JPEG or PNG.");