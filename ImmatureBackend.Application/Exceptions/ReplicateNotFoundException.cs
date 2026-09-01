namespace ImmatureBackend.Application.Exceptions;

public sealed class ReplicateNotFoundException(string message) : Exception(message);