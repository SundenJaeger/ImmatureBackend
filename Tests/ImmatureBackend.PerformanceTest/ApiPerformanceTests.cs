using ImmatureBackend.IntegrationTest.Infrastructure;
using NBomber.Contracts.Stats;
using NBomber.CSharp;

namespace ImmatureBackend.PerformanceTest;

[CollectionDefinition(Name)]
public sealed class PerformanceCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Performance";
}

[Collection(PerformanceCollection.Name)]
[Trait("Category", "Performance")]
public class ApiPerformanceTests(ApiFixture api)
{
    private const int SeedCount = 50;
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(15);

    private readonly HttpClient _capture = api.CreateClient(TestJwt.Authenticated());
    private readonly HttpClient _dashboard = api.CreateClient(TestJwt.Dashboard());

    [Fact]
    public async Task GetReplicates_Handles20RequestsPerSecond()
    {
        await SeedAsync();

        var stats = RunScenario("get_replicates", ratePerSecond: 20, () => _dashboard.GetAsync("/api/replicates"));

        AssertHealthy(stats, maxP95Ms: 500);
    }

    [Fact]
    public async Task GetSamples_Handles20RequestsPerSecond()
    {
        await SeedAsync();

        var stats = RunScenario("get_samples", ratePerSecond: 20, () => _dashboard.GetAsync("/api/samples"));

        AssertHealthy(stats, maxP95Ms: 500);
    }

    [Fact]
    public void SubmitReplicate_Handles10RequestsPerSecond()
    {
        var stats = RunScenario("submit_replicate", ratePerSecond: 10, async () =>
        {
            using var content = new ReplicateForm().ToContent();

            return await _capture.PostAsync("/api/replicate", content);
        });

        AssertHealthy(stats, maxP95Ms: 1000);
    }

    private async Task SeedAsync()
    {
        for (var i = 0; i < SeedCount; i++)
        {
            await _capture.SubmitAsync(new ReplicateForm());
        }
    }

    private static ScenarioStats RunScenario(string name, int ratePerSecond, Func<Task<HttpResponseMessage>> request)
    {
        var scenario = Scenario.Create(name, async _ =>
            {
                using var response = await request();
                var status = ((int)response.StatusCode).ToString();

                return response.IsSuccessStatusCode
                    ? Response.Ok(statusCode: status)
                    : Response.Fail(statusCode: status);
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(3))
            .WithLoadSimulations(
                Simulation.Inject(rate: ratePerSecond, interval: TimeSpan.FromSeconds(1), during: Duration));

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("nbomber-reports")
            .WithReportFileName(name)
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        return result.ScenarioStats.Single();
    }

    private static void AssertHealthy(ScenarioStats stats, double maxP95Ms)
    {
        Assert.True(stats.Ok.Request.Count > 0, "No request succeeded.");
        Assert.Equal(0, stats.Fail.Request.Count);
        Assert.True(stats.Ok.Latency.Percent95 <= maxP95Ms,
            $"p95 latency was {stats.Ok.Latency.Percent95} ms, limit is {maxP95Ms} ms.");
    }
}