using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var options = ParseArgs(args);
var baseUrl = options.GetValueOrDefault("base-url", Environment.GetEnvironmentVariable("HIRENIX_BASE_URL") ?? "http://localhost:5189").TrimEnd('/');
var duration = TimeSpan.Parse(options.GetValueOrDefault("duration", "00:01:00"));
var connections = int.Parse(options.GetValueOrDefault("connections", "25"));
var candidate = await LoginAsync(baseUrl, "candidate@hirenix.com", "Candidate@123");
var employer = await LoginAsync(baseUrl, "employer@hirenix.com", "Employer@123");
var conversationId = await CreateConversationAsync(baseUrl, candidate.Token, employer.UserId);
var stopAt = DateTimeOffset.UtcNow.Add(duration);
var latencies = new ConcurrentBag<long>();
var failures = 0;

await Parallel.ForEachAsync(Enumerable.Range(1, connections), async (index, cancellationToken) =>
{
    while (DateTimeOffset.UtcNow < stopAt && !cancellationToken.IsCancellationRequested)
    {
        try
        {
            var elapsed = await SendAndReceiveAsync(baseUrl, employer.Token, candidate.Token, conversationId, candidate.UserId, $"realtime-load-{index}-{Guid.NewGuid():N}", cancellationToken);
            latencies.Add(elapsed);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref failures);
            Console.Error.WriteLine(ex.Message);
        }
    }
});

var ordered = latencies.OrderBy(x => x).ToArray();
var total = ordered.Length + failures;
var errorRate = total == 0 ? 1 : failures / (double)total;
var p95 = Percentile(ordered, 0.95);
var p99 = Percentile(ordered, 0.99);
Console.WriteLine($"REALTIME_LOAD_DONE connections={connections} messages={ordered.Length} failures={failures} errorRate={errorRate:P2} p95Ms={p95} p99Ms={p99}");
if (ordered.Length == 0 || errorRate > 0.05 || p95 > 3000 || p99 > 5000)
{
    Environment.ExitCode = 1;
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
        var key = args[i][2..];
        result[key] = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[++i] : "true";
    }
    return result;
}

static async Task<(string Token, ulong UserId)> LoginAsync(string baseUrl, string identifier, string password)
{
    using var http = new HttpClient();
    using var response = await PostLoginWithRateLimitRetryAsync(http, baseUrl, identifier, password);
    var body = await response.Content.ReadAsStringAsync();
    response.EnsureSuccessStatusCode();
    using var document = JsonDocument.Parse(body);
    return (
        document.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!,
        document.RootElement.GetProperty("data").GetProperty("userId").GetUInt64());
}

static async Task<HttpResponseMessage> PostLoginWithRateLimitRetryAsync(HttpClient http, string baseUrl, string identifier, string password)
{
    var content = JsonSerializer.Serialize(new { identifier, password });
    var response = await http.PostAsync(
        $"{baseUrl}/api/Auth/login",
        new StringContent(content, Encoding.UTF8, "application/json"));
    if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
    {
        return response;
    }

    response.Dispose();
    await Task.Delay(TimeSpan.FromSeconds(65));
    return await http.PostAsync(
        $"{baseUrl}/api/Auth/login",
        new StringContent(content, Encoding.UTF8, "application/json"));
}

static async Task<ulong> CreateConversationAsync(string baseUrl, string candidateToken, ulong employerUserId)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", candidateToken);
    using var response = await http.PostAsync(
        $"{baseUrl}/api/messages/conversations",
        new StringContent(JsonSerializer.Serialize(new { participantUserId = employerUserId }), Encoding.UTF8, "application/json"));
    var body = await response.Content.ReadAsStringAsync();
    response.EnsureSuccessStatusCode();
    using var document = JsonDocument.Parse(body);
    return document.RootElement.GetProperty("id").GetUInt64();
}

static async Task<long> SendAndReceiveAsync(string baseUrl, string listenToken, string sendToken, ulong conversationId, ulong expectedSenderId, string content, CancellationToken cancellationToken)
{
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(TimeSpan.FromSeconds(10));
    using var socket = new ClientWebSocket();
    var websocketUrl = ToWebSocketUrl($"{baseUrl}/hubs/messages", listenToken);
    await socket.ConnectAsync(websocketUrl, timeout.Token);
    await SendFrameAsync(socket, "{\"protocol\":\"json\",\"version\":1}", timeout.Token);
    await WaitForHandshakeAsync(socket, timeout.Token);

    var stopwatch = Stopwatch.StartNew();
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sendToken);
    using var response = await http.PostAsync(
        $"{baseUrl}/api/messages/conversations/{conversationId}/items",
        new StringContent(JsonSerializer.Serialize(new { content }), Encoding.UTF8, "application/json"),
        timeout.Token);
    response.EnsureSuccessStatusCode();

    await WaitForMessageAsync(socket, expectedSenderId, content, timeout.Token);
    stopwatch.Stop();
    return stopwatch.ElapsedMilliseconds;
}

static Uri ToWebSocketUrl(string hubUrl, string token)
{
    var builder = new UriBuilder(hubUrl);
    builder.Scheme = builder.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
    builder.Query = "access_token=" + Uri.EscapeDataString(token);
    return builder.Uri;
}

static async Task SendFrameAsync(ClientWebSocket socket, string payload, CancellationToken cancellationToken)
{
    var bytes = Encoding.UTF8.GetBytes(payload + '\u001e');
    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
}

static async Task WaitForHandshakeAsync(ClientWebSocket socket, CancellationToken cancellationToken)
{
    await foreach (var frame in ReadFramesAsync(socket, cancellationToken))
    {
        if (string.IsNullOrWhiteSpace(frame)) continue;
        using var document = JsonDocument.Parse(frame);
        if (document.RootElement.TryGetProperty("error", out var error)) throw new InvalidOperationException(error.GetString());
        return;
    }
}

static async Task WaitForMessageAsync(ClientWebSocket socket, ulong expectedSenderId, string expectedContent, CancellationToken cancellationToken)
{
    await foreach (var frame in ReadFramesAsync(socket, cancellationToken))
    {
        if (string.IsNullOrWhiteSpace(frame) || frame == "{}") continue;
        using var document = JsonDocument.Parse(frame);
        var root = document.RootElement;
        if (!TryGetProperty(root, "type", out var type) || type.GetInt32() != 1) continue;
        if (!TryGetProperty(root, "target", out var target) || target.GetString() != "MessageReceived") continue;
        if (!TryGetProperty(root, "arguments", out var arguments) || arguments.GetArrayLength() == 0) continue;
        var message = arguments[0];
        var senderId = GetUInt64(message, "senderId", "SenderId");
        var content = GetString(message, "content", "Content");
        if (senderId == expectedSenderId && content == expectedContent) return;
    }
    throw new TimeoutException("MessageReceived event was not observed before the connection closed.");
}

static async IAsyncEnumerable<string> ReadFramesAsync(ClientWebSocket socket, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
{
    var buffer = new byte[8192];
    var text = new StringBuilder();
    while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType == WebSocketMessageType.Close) yield break;
        text.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
        if (!result.EndOfMessage) continue;
        var payload = text.ToString();
        text.Clear();
        foreach (var frame in payload.Split('\u001e', StringSplitOptions.RemoveEmptyEntries)) yield return frame;
    }
}

static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
{
    foreach (var property in element.EnumerateObject())
    {
        if (property.NameEquals(name) || property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            value = property.Value;
            return true;
        }
    }
    value = default;
    return false;
}

static ulong GetUInt64(JsonElement element, params string[] names)
{
    foreach (var name in names)
    {
        if (!TryGetProperty(element, name, out var value)) continue;
        return value.ValueKind == JsonValueKind.String ? ulong.Parse(value.GetString()!) : value.GetUInt64();
    }
    throw new InvalidOperationException("Expected sender id was not found.");
}

static string? GetString(JsonElement element, params string[] names)
{
    foreach (var name in names)
    {
        if (TryGetProperty(element, name, out var value)) return value.GetString();
    }
    return null;
}

static long Percentile(long[] values, double percentile)
{
    if (values.Length == 0) return long.MaxValue;
    var index = (int)Math.Ceiling(percentile * values.Length) - 1;
    return values[Math.Clamp(index, 0, values.Length - 1)];
}
