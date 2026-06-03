namespace Hirenix.SystemTests;

public sealed class SystemTestClient : IDisposable
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Token, ulong UserId)> TokenCache = new();

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri((Environment.GetEnvironmentVariable("HIRENIX_BASE_URL") ?? "http://localhost:5189").TrimEnd('/') + "/")
    };

    public HttpClient HttpClient => _httpClient;

    public async Task<(string Token, ulong UserId)> LoginAsync(string identifier, string password)
    {
        var cacheKey = $"{identifier}:{password}";
        if (TokenCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        using var response = await PostLoginWithRateLimitRetryAsync(identifier, password);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Login failed for {identifier}: {(int)response.StatusCode} {content}");
        using var document = JsonDocument.Parse(content);
        var data = document.RootElement.GetProperty("data");
        var result = (data.GetProperty("accessToken").GetString()!, data.GetProperty("userId").GetUInt64());
        TokenCache[cacheKey] = result;
        return result;
    }

    public async Task<JsonElement> GetJsonAsync(string path, string? token = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"GET {path} failed: {(int)response.StatusCode} {content}");
        return JsonDocument.Parse(content).RootElement.Clone();
    }

    public async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string path, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        return await _httpClient.SendAsync(request);
    }

    public void Dispose() => _httpClient.Dispose();

    private async Task<HttpResponseMessage> PostLoginWithRateLimitRetryAsync(string identifier, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { identifier, password });
        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return response;
        }

        response.Dispose();
        await Task.Delay(TimeSpan.FromSeconds(65));
        return await _httpClient.PostAsJsonAsync("api/Auth/login", new { identifier, password });
    }
}
