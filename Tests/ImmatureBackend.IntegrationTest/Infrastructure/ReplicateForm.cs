using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace ImmatureBackend.IntegrationTest.Infrastructure;

public sealed record ReplicateForm
{
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    public string TechnicianName { get; init; } = "Test Technician";
    public string SampleId { get; init; } = $"S{Guid.NewGuid():N}"[..13];
    public string Season { get; init; } = "2026WS";
    public string IndexNumber { get; init; } = Guid.NewGuid().ToString("N");
    public string SelectionName { get; init; } = "Selection A";
    public int SampleNo { get; init; } = 1;
    public string Weight { get; init; } = "0.9";
    public double Confidence { get; init; } = 0.9;

    public bool ManuallyOverridden { get; init; }
    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.UtcNow.AddSeconds(-30);
    public DateTimeOffset SubmittedAt { get; init; } = DateTimeOffset.UtcNow;
    public string ImageFileName { get; init; } = "grain.png";
    public bool IncludeImage { get; init; } = true;

    public MultipartFormDataContent ToContent(IDictionary<string, string>? overrides = null)
    {
        var grains = string.Create(CultureInfo.InvariantCulture,
            $$"""[{"x":10,"y":10,"width":20,"height":20,"confidence":{{Confidence}},"action":null}]""");

        var fields = new Dictionary<string, string>
        {
            ["TechnicianName"] = TechnicianName,
            ["SampleId"] = SampleId,
            ["AiPredictedGrains"] = grains,
            ["ConfirmedGrains"] = grains,
            ["Weight"] = Weight,
            ["Season"] = Season,
            ["IndexNumber"] = IndexNumber,
            ["SelectionName"] = SelectionName,
            ["SampleNo"] = SampleNo.ToString(CultureInfo.InvariantCulture),
            ["ManuallyOverridden"] = ManuallyOverridden ? "true" : "false",
            ["CapturedAt"] = CapturedAt.ToString("O", CultureInfo.InvariantCulture),
            ["SubmittedAt"] = SubmittedAt.ToString("O", CultureInfo.InvariantCulture)
        };

        foreach (var (name, value) in overrides ?? new Dictionary<string, string>())
        {
            fields[name] = value;
        }

        var content = new MultipartFormDataContent();

        foreach (var (name, value) in fields)
        {
            content.Add(new StringContent(value), name);
        }

        if (IncludeImage)
        {
            content.Add(PngContent(), "Image", ImageFileName);
        }

        return content;
    }

    public static ByteArrayContent PngContent()
    {
        var file = new ByteArrayContent(Png);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        return file;
    }
}

public static class HttpExtensions
{
    public static async Task<JsonNode> ReadJsonAsync(this HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }

    public static async Task<JsonNode> SubmitAsync(this HttpClient client, ReplicateForm form)
    {
        using var content = form.ToContent();
        var response = await client.PostAsync("/api/replicate", content);

        Assert.True(response.IsSuccessStatusCode,
            $"Submit failed: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");

        return await response.ReadJsonAsync();
    }

    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(body) };

        return client.SendAsync(request);
    }
}