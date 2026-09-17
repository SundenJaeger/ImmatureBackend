using FluentResults;

namespace ImmatureBackend.Application.Errors;

public class ReplicateNotFoundError(Guid id) : Error($"Replicated {id} was not found.");