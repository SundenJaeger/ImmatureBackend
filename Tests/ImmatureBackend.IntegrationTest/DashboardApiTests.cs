using System.Net;
using ImmatureBackend.IntegrationTest.Infrastructure;

namespace ImmatureBackend.IntegrationTest;

[Collection(ApiCollection.Name)]
public class DashboardApiTests(ApiFixture api)
{
    private readonly HttpClient _anonymous = api.CreateClient();
    private readonly HttpClient _capture = api.CreateClient(TestJwt.Authenticated());
    private readonly HttpClient _dashboard = api.CreateClient(TestJwt.Dashboard());

    [Theory]
    [InlineData("/api/replicates")]
    [InlineData("/api/samples")]
    [InlineData("/api/replicates/export")]
    public async Task DashboardEndpoint_WithoutToken_Returns401(string url)
    {
        var response = await _anonymous.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DashboardEndpoint_WithExpiredToken_Returns401()
    {
        var client = api.CreateClient(TestJwt.Expired());

        var response = await client.GetAsync("/api/replicates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/replicates")]
    [InlineData("/api/samples")]
    [InlineData("/api/replicates/export")]
    public async Task DashboardEndpoint_WithCaptureOnlyToken_Returns403(string url)
    {
        var response = await _capture.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DashboardEndpoint_WithAnonymousSupabaseUser_Returns403()
    {
        var client = api.CreateClient(TestJwt.Anonymous());

        var response = await client.GetAsync("/api/replicates");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetReplicates_ReturnsSubmittedReplicate()
    {
        var form = new ReplicateForm();
        var created = await _capture.SubmitAsync(form);

        var list = await (await _dashboard.GetAsync("/api/replicates")).ReadJsonAsync();
        var item = list.AsArray().Single(r => r!["id"]!.GetValue<string>() == created["id"]!.GetValue<string>())!;

        Assert.Equal(form.SampleId, item["sampleId"]!.GetValue<string>());
        Assert.Equal(form.TechnicianName, item["technicianName"]!.GetValue<string>());
        Assert.Equal(form.IndexNumber, item["indexNumber"]!.GetValue<string>());
        Assert.Equal("G1", item["grade"]!.GetValue<string>());
        Assert.Single(item["aiPredictedGrains"]!.AsArray());
    }

    [Fact]
    public async Task GetSamples_GroupsReplicatesOfTheSameSample()
    {
        var form = new ReplicateForm { Confidence = 0.5 };
        await _capture.SubmitAsync(form);
        await _capture.SubmitAsync(form);

        var samples = await (await _dashboard.GetAsync("/api/samples")).ReadJsonAsync();
        var sample = samples.AsArray().Single(s => s!["sampleId"]!.GetValue<string>() == form.SampleId)!;

        Assert.Equal(2, sample["replicateCount"]!.GetValue<int>());
        Assert.Equal(0, sample["goodCount"]!.GetValue<int>());
        Assert.Equal("review", sample["status"]!.GetValue<string>());
        Assert.Equal(2, sample["replicates"]!.AsArray().Count);
    }

    [Fact]
    public async Task UpdateStatus_ChangesStatusAndPersistsIt()
    {
        var created = await _capture.SubmitAsync(new ReplicateForm());
        var id = created["id"]!.GetValue<string>();

        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{id}/status", new { status = "rejected" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("rejected", (await response.ReadJsonAsync())["reviewStatus"]!.GetValue<string>());

        var list = await (await _dashboard.GetAsync("/api/replicates")).ReadJsonAsync();
        var item = list.AsArray().Single(r => r!["id"]!.GetValue<string>() == id)!;
        Assert.Equal("rejected", item["reviewStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task UpdateStatus_AcceptingTheOnlyPhotoOfASample_AcceptsTheSample()
    {
        var form = new ReplicateForm { Confidence = 0.5 };
        var photo = await _capture.SubmitAsync(form);

        await SetStatusAsync(photo, "accepted");

        var sample = await GetSampleAsync(form.SampleId);
        Assert.Equal("accepted", sample["status"]!.GetValue<string>());
        Assert.Equal(0.9m, sample["averageWeight"]!.GetValue<decimal>());
        Assert.Equal("G1", sample["averageGrade"]!.GetValue<string>());
    }

    [Fact]
    public async Task UpdateStatus_AcceptingEveryPhotoOfASample_AcceptsTheSampleOnlyAfterTheLastOne()
    {
        var form = new ReplicateForm { Confidence = 0.5 };
        var first = await _capture.SubmitAsync(form);
        var second = await _capture.SubmitAsync(form);

        await SetStatusAsync(first, "accepted");
        var afterFirst = await GetSampleAsync(form.SampleId);
        Assert.Equal("review", afterFirst["status"]!.GetValue<string>());
        Assert.Null(afterFirst["averageWeight"]);

        await SetStatusAsync(second, "accepted");
        var afterSecond = await GetSampleAsync(form.SampleId);
        Assert.Equal("accepted", afterSecond["status"]!.GetValue<string>());
        Assert.Equal(0.9m, afterSecond["averageWeight"]!.GetValue<decimal>());
    }

    [Fact]
    public async Task UpdateStatus_RejectedPhotoDoesNotBlockTheRestOfTheSample()
    {
        var form = new ReplicateForm { Confidence = 0.5 };
        var rejected = await _capture.SubmitAsync(form);
        var accepted = await _capture.SubmitAsync(form);

        await SetStatusAsync(rejected, "rejected");
        await SetStatusAsync(accepted, "accepted");

        var sample = await GetSampleAsync(form.SampleId);
        Assert.Equal("accepted", sample["status"]!.GetValue<string>());
        Assert.Equal(0.9m, sample["averageWeight"]!.GetValue<decimal>());
    }

    [Fact]
    public async Task UpdateStatus_RejectingTheOnlyPhoto_LeavesTheSampleInReview()
    {
        var form = new ReplicateForm { Confidence = 0.5 };
        var photo = await _capture.SubmitAsync(form);

        await SetStatusAsync(photo, "rejected");

        var sample = await GetSampleAsync(form.SampleId);
        Assert.Equal("review", sample["status"]!.GetValue<string>());
        Assert.Null(sample["averageWeight"]);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidStatus_Returns400()
    {
        var created = await _capture.SubmitAsync(new ReplicateForm());
        var id = created["id"]!.GetValue<string>();

        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{id}/status", new { status = "nonsense" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_UnknownId_Returns404()
    {
        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{Guid.NewGuid()}/status",
            new { status = "accepted" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithCaptureOnlyToken_Returns403()
    {
        var response = await _capture.PatchJsonAsync($"/api/replicates/{Guid.NewGuid()}/status",
            new { status = "accepted" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOverride_TurnsBadPhotoIntoGoodPhoto()
    {
        var created = await _capture.SubmitAsync(new ReplicateForm { Confidence = 0.5 });
        var id = created["id"]!.GetValue<string>();
        Assert.Equal("bad", created["quality"]!.GetValue<string>());

        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{id}/override", new { overridden = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.True(body["manuallyOverridden"]!.GetValue<bool>());
        Assert.Equal("good", body["quality"]!.GetValue<string>());
    }

    [Fact]
    public async Task UpdateOverride_MissingField_Returns400()
    {
        var created = await _capture.SubmitAsync(new ReplicateForm());
        var id = created["id"]!.GetValue<string>();

        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{id}/override", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOverride_UnknownId_Returns404()
    {
        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{Guid.NewGuid()}/override",
            new { overridden = true });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ExportCsv_ReturnsCsvContainingSubmittedReplicate()
    {
        var form = new ReplicateForm();
        await _capture.SubmitAsync(form);

        var response = await _dashboard.GetAsync("/api/replicates/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("granular-records-", response.Content.Headers.ContentDisposition!.ToString());

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("sample_id", csv);
        Assert.Contains(form.SampleId, csv);
    }

    private async Task SetStatusAsync(System.Text.Json.Nodes.JsonNode photo, string status)
    {
        var id = photo["id"]!.GetValue<string>();
        var response = await _dashboard.PatchJsonAsync($"/api/replicates/{id}/status", new { status });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<System.Text.Json.Nodes.JsonNode> GetSampleAsync(string sampleId)
    {
        var samples = await (await _dashboard.GetAsync("/api/samples")).ReadJsonAsync();

        return samples.AsArray().Single(s => s!["sampleId"]!.GetValue<string>() == sampleId)!;
    }
}