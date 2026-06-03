namespace Hirenix.SystemTests;

public sealed class SecurityTests
{
    [Fact]
    public async Task Auth_me_does_not_expose_sensitive_password_fields()
    {
        using var client = new SystemTestClient();
        var (token, _) = await client.LoginAsync("candidate@hirenix.com", "Candidate@123");
        using var response = await client.SendJsonAsync(HttpMethod.Get, "api/Auth/me", token: token);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, content);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password_hash", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Invalid_token_is_rejected()
    {
        using var client = new SystemTestClient();
        using var response = await client.SendJsonAsync(HttpMethod.Get, "api/Auth/me", token: "not-a-jwt");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Bad_password_login_is_rejected()
    {
        using var client = new SystemTestClient();
        using var response = await client.SendJsonAsync(HttpMethod.Post, "api/Auth/login", new { identifier = "candidate@hirenix.com", password = "WrongPassword@123" });
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.TooManyRequests,
            $"Expected 401 or 429, got {(int)response.StatusCode}");
    }
}
