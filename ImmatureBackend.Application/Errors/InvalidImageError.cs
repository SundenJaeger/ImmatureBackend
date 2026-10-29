using FluentResults;

namespace ImmatureBackend.Application.Errors;

public sealed class InvalidImageError(string message = "Image can only be JPEG or PNG") : Error(message);