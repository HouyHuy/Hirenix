namespace Hirenix.SystemTests;

public sealed class ApiContractTests
{
    [Fact]
    public async Task Swagger_document_contains_core_backend_paths()
    {
        using var client = new SystemTestClient();
        var swagger = await client.GetJsonAsync("swagger/v1/swagger.json");
        var paths = swagger.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/api/jobs", out _));
        Assert.True(paths.TryGetProperty("/api/Auth/login", out _));
        Assert.True(paths.TryGetProperty("/api/Applications", out _));
        Assert.True(paths.TryGetProperty("/api/employer/applications", out _));
        Assert.True(paths.TryGetProperty("/api/messages/conversations", out _));
    }

    [Fact]
    public async Task Public_taxonomy_and_jobs_endpoints_return_expected_schema()
    {
        using var client = new SystemTestClient();
        var jobs = await client.GetJsonAsync("api/Jobs?page=1&pageSize=5");
        Assert.True(jobs.GetProperty("success").GetBoolean());
        Assert.True(jobs.GetProperty("data").TryGetProperty("data", out var jobItems));
        Assert.Equal(JsonValueKind.Array, jobItems.ValueKind);

        var industries = await client.GetJsonAsync("api/taxonomy/industries");
        Assert.True(industries.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Array, industries.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task Candidate_token_can_access_candidate_job_detail()
    {
        using var client = new SystemTestClient();
        var (candidateToken, _) = await client.LoginAsync("candidate@hirenix.com", "Candidate@123");
        var jobs = await client.GetJsonAsync("api/Jobs?page=1&pageSize=1");
        var firstJob = jobs.GetProperty("data").GetProperty("data").EnumerateArray().FirstOrDefault();
        Assert.NotEqual(default, firstJob);

        var detail = await client.GetJsonAsync($"api/Jobs/{firstJob.GetProperty("id").GetUInt64()}/detail", candidateToken);
        Assert.True(detail.GetProperty("success").GetBoolean());
        Assert.True(detail.GetProperty("data").TryGetProperty("hasApplied", out _));
    }
}
