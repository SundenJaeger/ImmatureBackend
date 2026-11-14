namespace ImmatureBackend.Application.Exceptions;

public sealed class DuplicateReplicateException(Exception innerException)
    : Exception("A replicate with the same season, index number, sample number and replicate number already exists.",
        innerException);