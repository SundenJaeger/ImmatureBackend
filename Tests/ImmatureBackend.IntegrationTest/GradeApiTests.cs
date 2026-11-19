using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using ImmatureBackend.IntegrationTest.Infrastructure;

namespace ImmatureBackend.IntegrationTest;

[Collection(ApiCollection.Name)]
public class GradeApiTests(ApiFixture api)
{
    private const string ValidUrl = "/api/grade?weight=0.47";

    private readonly HttpClient _noToken = api.CreateClient();
    private readonly HttpClient _capture = api.CreateClient(TestJwt.Authenticated());
    private readonly HttpClient _dashboard = api.CreateClient(TestJwt.Dashboard());

    private static decimal Dec(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    // ---- security

    [Fact]
    public async Task Grade_WithoutToken_Returns401()
    {
        var response = await _noToken.GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Grade_WithExpiredToken_Returns401()
    {
        var response = await api.CreateClient(TestJwt.Expired()).GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Grade_WithTamperedSignature_Returns401()
    {
        var token = TestJwt.Authenticated();
        var tampered = token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA");

        var response = await api.CreateClient(tampered).GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Grade_WithAnonymousSupabaseUser_Returns200()
    {
        var response = await api.CreateClient(TestJwt.Anonymous()).GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Grade_WithCaptureUser_Returns200()
    {
        var response = await _capture.GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Grade_WithDashboardUser_Returns200()
    {
        var response = await _dashboard.GetAsync(ValidUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Grade_HasSameAuthenticationOutcomesAsPredict()
    {
        var clients = new[] { _noToken, api.CreateClient(TestJwt.Expired()), api.CreateClient(TestJwt.Anonymous()) };

        foreach (var client in clients)
        {
            using var predictForm = new MultipartFormDataContent
            {
                { ReplicateForm.PngContent(), "Image", "grain.png" },
                { new StringContent("Test Technician"), "TechnicianName" },
                { new StringContent("S-parity-1"), "SampleId" }
            };

            var predict = await client.PostAsync("/api/predict", predictForm);
            var grade = await client.GetAsync(ValidUrl);

            Assert.Equal(predict.StatusCode, grade.StatusCode);
        }
    }

    // ---- behaviour

    [Fact]
    public async Task Grade_ValidWeight_ReturnsWeightPercentageGradeAndRecommendedValue()
    {
        var response = await _capture.GetAsync(ValidUrl);
        var body = await response.ReadJsonAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0.47m, body["weight"]!.GetValue<decimal>());
        Assert.Equal(1.6m, body["percentage"]!.GetValue<decimal>());
        Assert.Equal("Pr", body["grade"]!.GetValue<string>());
        Assert.True(body["meetsRecommendedValue"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("0.58", "1.9", "Pr")]
    [InlineData("0.59", "2.0", "G1")]
    [InlineData("1.51", "5.0", "G1")]
    [InlineData("1.52", "5.1", "G2")]
    [InlineData("3.01", "10.0", "G2")]
    [InlineData("3.02", "10.1", "G3")]
    [InlineData("4.51", "15.0", "G3")]
    [InlineData("4.52", "15.1", "Not Classified")]
    [InlineData("0.045", "0.2", "Pr")]
    [InlineData("0.01", "0.0", "Pr")]
    [InlineData("30", "100.0", "Not Classified")]
    public async Task Grade_ReturnsPercentageAndGradeAtEachBoundary(string weight, string percentage, string grade)
    {
        var body = await (await _capture.GetAsync($"/api/grade?weight={weight}")).ReadJsonAsync();

        Assert.Equal(Dec(percentage), body["percentage"]!.GetValue<decimal>());
        Assert.Equal(grade, body["grade"]!.GetValue<string>());
    }

    [Fact]
    public async Task Grade_ResponseContainsOnlyTheDocumentedProperties()
    {
        var body = (JsonObject)await (await _capture.GetAsync(ValidUrl)).ReadJsonAsync();

        Assert.Equal(
            new[] { "grade", "meetsRecommendedValue", "percentage", "weight" },
            body.Select(property => property.Key).Order().ToArray());
    }

    // ---- validation

    [Theory]
    [InlineData("/api/grade", "Weight is required.")]
    [InlineData("/api/grade?weight=0", "Weight must be greater than zero.")]
    [InlineData("/api/grade?weight=-1", "Weight must be greater than zero.")]
    [InlineData("/api/grade?weight=30.01", "Weight cannot be more than 30 g.")]
    [InlineData("/api/grade?weight=31", "Weight cannot be more than 30 g.")]
    public async Task Grade_InvalidWeight_Returns400WithMessage(string url, string message)
    {
        var response = await _capture.GetAsync(url);
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(message, text);
        Assert.DoesNotContain("traceId", text);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public async Task Grade_UnparseableWeight_Returns400(string weight)
    {
        var response = await _capture.GetAsync($"/api/grade?weight={Uri.EscapeDataString(weight)}");
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("traceId", text);
    }

    // The query binder uses the invariant culture, which reads "," as a thousands separator rather than a
    // decimal point: "0,5" binds as 5 g. Pinned here so clients know to always send the weight with a dot.
    [Fact]
    public async Task Grade_CommaIsReadAsThousandsSeparator()
    {
        var accepted = await _capture.GetAsync($"/api/grade?weight={Uri.EscapeDataString("0,5")}");
        var rejected = await _capture.GetAsync($"/api/grade?weight={Uri.EscapeDataString("0,47")}");

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(5m, (await accepted.ReadJsonAsync())["weight"]!.GetValue<decimal>());
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains("Weight cannot be more than 30 g.", await rejected.Content.ReadAsStringAsync());
    }

    // ---- non-destructive

    [Fact]
    public async Task Grade_DoesNotSaveAnything()
    {
        var before = (await (await _dashboard.GetAsync("/api/replicates")).ReadJsonAsync()).AsArray().Count;

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await _capture.GetAsync(ValidUrl)).StatusCode);
        }

        var after = (await (await _dashboard.GetAsync("/api/replicates")).ReadJsonAsync()).AsArray().Count;

        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Grade_PostMethod_Returns405()
    {
        var response = await _capture.PostAsync(ValidUrl, content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("0.3")]
    [InlineData("0.9")]
    [InlineData("2.4")]
    [InlineData("3.9")]
    [InlineData("6.0")]
    public async Task Grade_MatchesGradeStoredBySubmission(string weight)
    {
        var preview = await (await _capture.GetAsync($"/api/grade?weight={weight}")).ReadJsonAsync();
        var submitted = await _capture.SubmitAsync(new ReplicateForm { Weight = weight });

        Assert.Equal(submitted["grade"]!.GetValue<string>(), preview["grade"]!.GetValue<string>());
        Assert.Equal(submitted["percentage"]!.GetValue<decimal>(), preview["percentage"]!.GetValue<decimal>());
    }
}
