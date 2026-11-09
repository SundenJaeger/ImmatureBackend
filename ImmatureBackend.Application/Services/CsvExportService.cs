using System.Globalization;
using CsvHelper;
using ImmatureBackend.Application.Exports;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ImmatureBackend.Application.Services;

public class CsvExportService(IReplicateService replicateService) : ICsvExportService
{
    private const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private static readonly CsvWriterOptions WriterOptions = new()
    {
        WriteByteOrderMark = true,
        Injection = CsvInjection.Escape
    };

    public async Task<byte[]> ExportReplicatesAsync(CancellationToken cancellationToken = default)
    {
        var items = await replicateService.GetAllReplicateListItemsAsync(cancellationToken);

        using var stream = new MemoryStream();

        await using (var csv = CsvWriter.Create(stream, WriterOptions, leaveOpen: true))
        {
            csv.WriteHeader<ReplicateCsvRow>();

            foreach (var item in items)
            {
                await csv.WriteRecordAsync(ToRow(item), cancellationToken);
            }
        }

        return stream.ToArray();
    }

    private static ReplicateCsvRow ToRow(ReplicateListItem item)
    {
        return new ReplicateCsvRow
        {
            Id = item.Id,
            TechnicianName = item.TechnicianName,
            Season = item.Season ?? string.Empty,
            IndexNumber = item.IndexNumber ?? string.Empty,
            SelectionName = item.SelectionName ?? string.Empty,
            SampleId = item.SampleId,
            SampleNo = item.SampleNo,
            ImmatureWeight = item.ImmatureWeight,
            Percentage = item.Percentage,
            Grade = GradeText(item),
            MeetsRecommendedValue = item.MeetsRecommendedValue,
            ReviewStatus = item.ReviewStatus.ToString().ToLowerInvariant(),
            CapturedAt = FormatTimestamp(item.CapturedAt),
            SubmittedAt = FormatTimestamp(item.SubmittedAt),
            ProcessingSeconds = item.ProcessingSeconds,
            CreatedAt = FormatTimestamp(item.CreatedAt),
            ImageSecureUrl = item.ImageSecureUrl,
            AiPredictedGrains = JsonConvert.SerializeObject(item.AiPredictedGrains, JsonSettings),
            ConfirmedGrains = JsonConvert.SerializeObject(item.ConfirmedGrains, JsonSettings)
        };
    }

    private static string GradeText(ReplicateListItem item)
    {
        return JsonConvert.SerializeObject(item.Grade).Trim('"');
    }

    private static string FormatTimestamp(DateTime? value)
    {
        return value is null
            ? string.Empty
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                .ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }
}