namespace Hirenix.SystemTests;

public sealed class BusinessLogicBoundaryTests
{
    [Fact]
    public async Task Candidate_cannot_access_employer_application_list()
    {
        using var client = new SystemTestClient();
        var (candidateToken, _) = await client.LoginAsync("candidate@hirenix.com", "Candidate@123");
        using var response = await client.SendJsonAsync(HttpMethod.Get, "api/employer/applications", token: candidateToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Employer_job_create_rejects_invalid_salary_range()
    {
        using var client = new SystemTestClient();
        var (employerToken, _) = await client.LoginAsync("employer@hirenix.com", "Employer@123");
        var industries = await client.GetJsonAsync("api/taxonomy/industries");
        var locations = await client.GetJsonAsync("api/taxonomy/locations");
        var industryId = industries.GetProperty("data").EnumerateArray().First().GetProperty("id").GetUInt32();
        var locationId = locations.GetProperty("data").EnumerateArray().First().GetProperty("id").GetUInt32();

        var payload = new
        {
            title = "Invalid Salary Test",
            description = "Boundary test",
            requirements = "Boundary test",
            industryId,
            locationId,
            workType = "Fulltime",
            level = "Junior",
            salaryMin = 2000,
            salaryMax = 1000,
            isRemote = true,
            expiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)).ToString("yyyy-MM-dd"),
            skillIds = Array.Empty<uint>()
        };

        using var response = await client.SendJsonAsync(HttpMethod.Post, "api/employer/jobs", payload, employerToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Employer_job_create_rejects_expiry_date_more_than_ninety_days()
    {
        using var client = new SystemTestClient();
        var (employerToken, _) = await client.LoginAsync("employer@hirenix.com", "Employer@123");
        var industries = await client.GetJsonAsync("api/taxonomy/industries");
        var locations = await client.GetJsonAsync("api/taxonomy/locations");
        var industryId = industries.GetProperty("data").EnumerateArray().First().GetProperty("id").GetUInt32();
        var locationId = locations.GetProperty("data").EnumerateArray().First().GetProperty("id").GetUInt32();

        var payload = new
        {
            title = "Invalid Expiry Test",
            description = "Boundary test",
            requirements = "Boundary test",
            industryId,
            locationId,
            workType = "Fulltime",
            level = "Junior",
            salaryMin = 1000,
            salaryMax = 2000,
            isRemote = true,
            expiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120)).ToString("yyyy-MM-dd"),
            skillIds = Array.Empty<uint>()
        };

        using var response = await client.SendJsonAsync(HttpMethod.Post, "api/employer/jobs", payload, employerToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
