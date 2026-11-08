namespace ImmatureBackend.Application.Exceptions;

public sealed class DuplicateReplicateException(Exception innerException)
    : Exception("A replicate with the same season, index number and sample number already exists.", innerException);