using System.Net;
using ImmatureBackend.IntegrationTest.Infrastructure;

namespace ImmatureBackend.IntegrationTest;

[Collection(ApiCollection.Name)]
public class PredictAndHealthTests(ApiFixture api)
{
    private readonly HttpClient _anonymous = api.CreateClient();
    private readonly HttpClient _capture = api.CreateClient(TestJwt.Authenticated());

    private static MultipartFormDataContent PredictForm(bool includeTechnician = true, bool emptyImage = false)
    {
        var content = new MultipartFormDataContent
        {
            { emptyImage ? new ByteArrayContent([]) : ReplicateForm.PngContent(), "Image", "grain.png" },
            { new StringContent("S-predict-1"), "SampleId" }
        };

        if (includeTechnician)
        {
            content.Add(new StringContent("Test Technician"), "TechnicianName");
        }

        return content;
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _anonymous.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Predict_WithoutToken_Returns401()
    {
        using var content = PredictForm();

        var response = await _anonymous.PostAsync("/api/predict", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Predict_ValidRequest_ReturnsDetectedGrains()
    {
        using var content = PredictForm();

        var response = await _capture.PostAsync("/api/predict", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Single(body["grains"]!.AsArray());
    }

    [Fact]
    public async Task Predict_MissingTechnicianName_Returns400()
    {
        using var content = PredictForm(includeTechnician: false);

        var response = await _capture.PostAsync("/api/predict", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Predict_EmptyImage_Returns400()
    {
        using var content = PredictForm(emptyImage: true);

        var response = await _capture.PostAsync("/api/predict", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}