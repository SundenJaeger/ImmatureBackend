using FluentResults;

namespace ImmatureBackend.Application.Errors;

public sealed class ReplicateNumberConflictError(string season, string indexNumber, int sampleNo)
    : Error(
        $"Could not assign a replicate number for sample {sampleNo}, season {season}, index number {indexNumber}. Please retry.");