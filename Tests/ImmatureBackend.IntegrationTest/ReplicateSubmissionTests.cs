using System.Net;
using ImmatureBackend.IntegrationTest.Infrastructure;

namespace ImmatureBackend.IntegrationTest;

[Collection(ApiCollection.Name)]
public class ReplicateSubmissionTests(ApiFixture api)
{
    private readonly HttpClient _anonymous = api.CreateClient();
    private readonly HttpClient _capture = api.CreateClient(TestJwt.Authenticated());
    private readonly HttpClient _dashboard = api.CreateClient(TestJwt.Dashboard());

    [Fact]
    public async Task Submit_ValidReplicate_ReturnsGradeAndFirstReplicateNumber()
    {
        var form = new ReplicateForm();

        var body = await _capture.SubmitAsync(form);

        Assert.Equal(3.0m, body["percentage"]!.GetValue<decimal>());
        Assert.Equal("G1", body["grade"]!.GetValue<string>());
        Assert.False(body["meetsRecommendedValue"]!.GetValue<bool>());
        Assert.Equal(1, body["replicateNo"]!.GetValue<int>());
        Assert.Equal($"{form.SampleId}-01", body["replicateLabel"]!.GetValue<string>());
        Assert.Equal("good", body["quality"]!.GetValue<string>());
        Assert.Equal("review", body["reviewStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_WithoutToken_Returns401()
    {
        using var content = new ReplicateForm().ToContent();

        var response = await _anonymous.PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Submit_WithExpiredToken_Returns401()
    {
        using var content = new ReplicateForm().ToContent();

        var response = await api.CreateClient(TestJwt.Expired()).PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("0.3", "Pr")]
    [InlineData("0.9", "G1")]
    [InlineData("2.4", "G2")]
    [InlineData("3.9", "G3")]
    [InlineData("6.0", "Not Classified")]
    public async Task Submit_AssignsGradeFromWeight(string weight, string expectedGrade)
    {
        var body = await _capture.SubmitAsync(new ReplicateForm { Weight = weight });

        Assert.Equal(expectedGrade, body["grade"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_SecondPhotoOfSameSample_GetsNextReplicateNumber()
    {
        var form = new ReplicateForm();

        await _capture.SubmitAsync(form);
        var second = await _capture.SubmitAsync(form);

        Assert.Equal(2, second["replicateNo"]!.GetValue<int>());
        Assert.Equal($"{form.SampleId}-02", second["replicateLabel"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_SameIndexNumberButDifferentSampleNo_StartsOverAtOne()
    {
        var form = new ReplicateForm { SampleNo = 1 };

        await _capture.SubmitAsync(form);
        var other = await _capture.SubmitAsync(form with { SampleNo = 2 });

        Assert.Equal(1, other["replicateNo"]!.GetValue<int>());
    }

    [Fact]
    public async Task Submit_LowConfidencePhoto_IsBadAndStaysInReview()
    {
        var body = await _capture.SubmitAsync(new ReplicateForm { Confidence = 0.5 });

        Assert.Equal("bad", body["quality"]!.GetValue<string>());
        Assert.Equal("review", body["reviewStatus"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_LowConfidenceButManuallyOverridden_IsGood()
    {
        var body = await _capture.SubmitAsync(new ReplicateForm { Confidence = 0.5, ManuallyOverridden = true });

        Assert.Equal("good", body["quality"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_TwoGoodPhotos_AutoAcceptsSampleAndAveragesIt()
    {
        var form = new ReplicateForm();

        var first = await _capture.SubmitAsync(form);
        var second = await _capture.SubmitAsync(form);

        Assert.Equal("review", first["reviewStatus"]!.GetValue<string>());
        Assert.Equal("accepted", second["reviewStatus"]!.GetValue<string>());

        var samples = await (await _dashboard.GetAsync("/api/samples")).ReadJsonAsync();
        var sample = samples.AsArray().Single(s => s!["sampleId"]!.GetValue<string>() == form.SampleId)!;

        Assert.Equal("accepted", sample["status"]!.GetValue<string>());
        Assert.Equal(2, sample["replicateCount"]!.GetValue<int>());
        Assert.Equal(2, sample["goodCount"]!.GetValue<int>());
        Assert.Equal(0.9m, sample["averageWeight"]!.GetValue<decimal>());
        Assert.Equal("G1", sample["averageGrade"]!.GetValue<string>());
    }

    [Fact]
    public async Task Submit_ThreeAtTheSameTime_GetDistinctReplicateNumbers()
    {
        var form = new ReplicateForm();

        var results = await Task.WhenAll(
            _capture.SubmitAsync(form),
            _capture.SubmitAsync(form),
            _capture.SubmitAsync(form));

        var numbers = results.Select(r => r["replicateNo"]!.GetValue<int>()).Order().ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, numbers);
    }

    [Theory]
    [InlineData("Weight", "0")]
    [InlineData("Weight", "-1")]
    [InlineData("Weight", "30.5")]
    [InlineData("Season", "2026XX")]
    [InlineData("Season", "26WS")]
    [InlineData("SampleNo", "3")]
    [InlineData("SampleNo", "0")]
    [InlineData("TechnicianName", "")]
    [InlineData("SampleId", "")]
    [InlineData("IndexNumber", "")]
    [InlineData("SelectionName", "")]
    public async Task Submit_InvalidField_Returns400(string field, string value)
    {
        using var content = new ReplicateForm().ToContent(new Dictionary<string, string> { [field] = value });

        var response = await _capture.PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_SubmittedBeforeCaptured_Returns400()
    {
        var now = DateTimeOffset.UtcNow;
        using var content = new ReplicateForm { CapturedAt = now, SubmittedAt = now.AddMinutes(-5) }.ToContent();

        var response = await _capture.PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_WithoutImage_Returns400()
    {
        using var content = new ReplicateForm { IncludeImage = false }.ToContent();

        var response = await _capture.PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_WhenImageUploadFails_Returns422AndSavesNothing()
    {
        var form = new ReplicateForm { ImageFileName = FakeImageService.FailingFileName };
        using var content = form.ToContent();

        var response = await _capture.PostAsync("/api/replicate", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var list = await (await _dashboard.GetAsync("/api/replicates")).ReadJsonAsync();
        Assert.DoesNotContain(list.AsArray(), r => r!["sampleId"]!.GetValue<string>() == form.SampleId);
    }
}