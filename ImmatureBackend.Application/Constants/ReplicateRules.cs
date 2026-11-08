namespace ImmatureBackend.Application.Constants;

public static class ReplicateRules
{
    public const decimal SampleWeightGrams = 30m;
    public const decimal RecommendedValuePercentage = 2.0m;
    public const long MaxImageBytes = 10 * 1024 * 1024;
    public const string SeasonPattern = @"^\d{4}(DS|WS)$";

    public const int TechnicianNameMaxLength = 100;
    public const int SeasonMaxLength = 6;
    public const int IndexNumberMaxLength = 50;
    public const int SelectionNameMaxLength = 100;

    public const int MinSampleNo = 1;
    public const int MaxSampleNo = 2;
}