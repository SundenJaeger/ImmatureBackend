using FluentResults;

namespace ImmatureBackend.Application.Errors;

public sealed class DuplicateSampleError(string season, string indexNumber, int sampleNo)
    : Error($"Sample {sampleNo} already exists for season {season}, index number {indexNumber}.");