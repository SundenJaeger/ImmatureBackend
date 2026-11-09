using System.Text;
using CsvHelper;
using FluentAssertions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Application.Services;
using ImmatureBackend.Domain.Enums;
using Moq;

namespace ImmatureBackend.Test.Application.Services;

public class CsvExportServiceTests
{
    private readonly Mock<IReplicateService> _replicateService = new();
    private readonly CsvExportService _service;

    public CsvExportServiceTests()
    {
        _service = new CsvExportService(_replicateService.Object);
    }

    private static ReplicateListItem CreateItem(Action<ReplicateListItem>? customize = null)
    {
        var item = new ReplicateListItem
        {
            Id = Guid.NewGuid().ToString(),
            TechnicianName = "Tech1",
            CreatedAt = new DateTime(2026, 11, 9, 8, 10, 0, DateTimeKind.Utc),
            SampleId = "S1",
            AiPredictedGrains = [new GrainBox { X = 1 }],
            ConfirmedGrains = [new GrainBox { X = 2 }],
            ImmatureWeight = 1.5m,
            Percentage = 5.0m,
            Grade = Grade.G1,
            ReviewStatus = ReviewStatus.Review,
            MeetsRecommendedValue = false,
            Season = "2026WS",
            IndexNumber = "1234",
            SelectionName = "NSIC Rc 222",
            SampleNo = 1,
            CapturedAt = new DateTime(2026, 11, 9, 8, 0, 0, DateTimeKind.Utc),
            SubmittedAt = new DateTime(2026, 11, 9, 8, 5, 0, DateTimeKind.Utc),
            ProcessingSeconds = 300m,
            ImagePublicId = "immature/images/abc",
            ImageSecureUrl = "https://cdn.example/abc.jpg"
        };

        customize?.Invoke(item);

        return item;
    }

    private async Task<(byte[] Bytes, List<Dictionary<string, string>> Rows)> ExportAsync(params ReplicateListItem[] items)
    {
        _replicateService
            .Setup(s => s.GetAllReplicateListItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var bytes = await _service.ExportReplicatesAsync();

        await using var csv = CsvReader.Create(new MemoryStream(bytes));
        var rows = csv.GetRecords<CsvDynamicRecord>()
            .Select(record => record.ToDictionary().ToDictionary(p => p.Key, p => p.Value ?? string.Empty))
            .ToList();

        return (bytes, rows);
    }

    [Fact]
    public async Task Export_StartsWithUtf8Bom()
    {
        var (bytes, _) = await ExportAsync(CreateItem());

        bytes.Take(3).Should().Equal(0xEF, 0xBB, 0xBF);
    }

    [Fact]
    public async Task Export_NoRecords_ReturnsHeaderOnly()
    {
        var (bytes, rows) = await ExportAsync();

        rows.Should().BeEmpty();
        Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF').Should().StartWith(
            "id,technician_name,season,index_number,selection_name,sample_id,sample_no,immature_weight,percentage,grade,"
            + "meets_recommended_value,review_status,captured_at,submitted_at,processing_seconds,created_at,"
            + "image_secure_url,ai_predicted_grains,confirmed_grains");
    }

    [Fact]
    public async Task Export_WritesFieldsInTheExpectedFormat()
    {
        var item = CreateItem();

        var (_, rows) = await ExportAsync(item);

        var row = rows.Should().ContainSingle().Subject;
        row["id"].Should().Be(item.Id);
        row["season"].Should().Be("2026WS");
        row["index_number"].Should().Be("1234");
        row["sample_no"].Should().Be("1");
        row["immature_weight"].Should().Be("1.5");
        row["percentage"].Should().Be("5.0");
        row["grade"].Should().Be("G1");
        row["meets_recommended_value"].Should().Be("False");
        row["review_status"].Should().Be("review");
        row["captured_at"].Should().Be("2026-11-09T08:00:00.000Z");
        row["submitted_at"].Should().Be("2026-11-09T08:05:00.000Z");
        row["processing_seconds"].Should().Be("300");
        row["technician_name"].Should().Be("Tech1");
        row["created_at"].Should().Be("2026-11-09T08:10:00.000Z");
        row["image_secure_url"].Should().Be("https://cdn.example/abc.jpg");
    }

    [Fact]
    public async Task Export_NotClassifiedGrain_UsesApiText()
    {
        var (_, rows) = await ExportAsync(CreateItem(i => i.Grade = Grade.NotClassified));

        rows.Single()["grade"].Should().Be("Not Classified");
    }

    [Fact]
    public async Task Export_GrainListsAreJsonTextInsideTheirCells()
    {
        var (_, rows) = await ExportAsync(CreateItem());

        rows.Single()["ai_predicted_grains"].Should().StartWith("[{").And.Contain("\"x\"");
        rows.Single()["confirmed_grains"].Should().StartWith("[{");
    }

    [Fact]
    public async Task Export_CommasQuotesAndNewlines_AreQuotedAndRoundTrip()
    {
        const string tricky = "Line \"A\", part 2\nsecond line";

        var (_, rows) = await ExportAsync(CreateItem(i => i.SelectionName = tricky));

        rows.Single()["selection_name"].Should().Be(tricky);
    }

    [Fact]
    public async Task Export_CellStartingWithFormulaCharacter_IsEscaped()
    {
        var (_, rows) = await ExportAsync(CreateItem(i => i.SelectionName = "=HYPERLINK(\"http://evil\")"));

        rows.Single()["selection_name"].Should().StartWith("'=");
    }

    [Fact]
    public async Task Export_MissingTimestamps_LeavesTimestampCellsEmpty()
    {
        var (_, rows) = await ExportAsync(CreateItem(i =>
        {
            i.CapturedAt = null;
            i.SubmittedAt = null;
        }));

        var row = rows.Single();
        row["captured_at"].Should().BeEmpty();
        row["submitted_at"].Should().BeEmpty();
    }
}