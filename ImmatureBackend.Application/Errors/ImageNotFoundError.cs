using FluentResults;

namespace ImmatureBackend.Application.Errors;

public sealed class ImageNotFoundError(Guid id) : Error($"Image {id} was not found.");