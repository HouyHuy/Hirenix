using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var options = ParseArgs(args);
var hubUrl = Required(options, "hub");
var listenToken = Required(options, "listen-token");
var sendUrl = Required(options, "send-url");
var sendToken = Required(options, "send-token");
var content = Required(options, "content");
var expectFrom = ulong.Parse(Required(options, "expect-from"));
var timeoutSeconds = int.Parse(options.GetValueOrDefault("timeout", "15"));

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
using var socket = new ClientWebSocket();
var websocketUrl = ToWebSocketUrl(hubUrl, listenToken);
await socket.ConnectAsync(websocketUrl, timeout.Token);
await SendFrameAsync(socket, "{\"protocol\":\"json\",\"version\":1}", timeout.Token);
await WaitForHandshakeAsync(socket, timeout.Token);
Console.WriteLine("SIGNALR_SMOKE_CONNECTED");

using var http = new HttpClient();
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sendToken);
using var response = await http.PostAsync(
    sendUrl,
    new StringContent(JsonSerializer.Serialize(new { content }), Encoding.UTF8, "application/json"),
    timeout.Token);
var responseBody = await response.Content.ReadAsStringAsync(timeout.Token);
if (!response.IsSuccessStatusCode)
{
    throw new InvalidOperationException($"Send message failed: {(int)response.StatusCode} {responseBody}");
}
Console.WriteLine("SIGNALR_SMOKE_SENT");

await WaitForMessageAsync(socket, expectFrom, content, timeout.Token);
Console.WriteLine("SIGNALR_SMOKE_RECEIVED");

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < args.Length; index++)
    {
        var current = args[index];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            result[key] = "true";
            continue;
        }

        result[key] = args[++index];
    }

    return result;
}

static string Required(IReadOnlyDictionary<string, string> options, string key)
{
    if (!options.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
    {
        throw new ArgumentException($"Missing required --{key} argument");
    }

    return value;
}

static Uri ToWebSocketUrl(string hubUrl, string token)
{
    var builder = new UriBuilder(hubUrl);
    builder.Scheme = builder.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
    var separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : builder.Query.TrimStart('?') + "&";
    builder.Query = separator + "access_token=" + Uri.EscapeDataString(token);
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
        if (string.IsNullOrWhiteSpace(frame))
        {
            continue;
        }

        using var document = JsonDocument.Parse(frame);
        if (document.RootElement.TryGetProperty("error", out var error))
        {
            throw new InvalidOperationException("SignalR handshake failed: " + error.GetString());
        }

        return;
    }
}

static async Task WaitForMessageAsync(ClientWebSocket socket, ulong expectFrom, string expectedContent, CancellationToken cancellationToken)
{
    await foreach (var frame in ReadFramesAsync(socket, cancellationToken))
    {
        if (string.IsNullOrWhiteSpace(frame) || frame == "{}")
        {
            continue;
        }

        using var document = JsonDocument.Parse(frame);
        var root = document.RootElement;
        if (!TryGetProperty(root, "type", out var type) || type.GetInt32() != 1)
        {
            continue;
        }

        if (!TryGetProperty(root, "target", out var target) || target.GetString() != "MessageReceived")
        {
            continue;
        }

        if (!TryGetProperty(root, "arguments", out var arguments) || arguments.GetArrayLength() == 0)
        {
            continue;
        }

        var message = arguments[0];
        var senderId = GetUInt64(message, "senderId", "SenderId");
        var content = GetString(message, "content", "Content");
        if (senderId == expectFrom && content == expectedContent)
        {
            return;
        }
    }

    throw new TimeoutException("SignalR message was not received before the connection closed.");
}

static async IAsyncEnumerable<string> ReadFramesAsync(ClientWebSocket socket, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
{
    var buffer = new byte[8192];
    var text = new StringBuilder();

    while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            yield break;
        }

        text.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
        if (!result.EndOfMessage)
        {
            continue;
        }

        var payload = text.ToString();
        text.Clear();
        var frames = payload.Split('\u001e', StringSplitOptions.RemoveEmptyEntries);
        foreach (var frame in frames)
        {
            yield return frame;
        }
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
        if (TryGetProperty(element, name, out var value))
        {
            return value.ValueKind == JsonValueKind.String ? ulong.Parse(value.GetString()!) : value.GetUInt64();
        }
    }

    throw new InvalidOperationException("Expected numeric JSON property was not found: " + string.Join(", ", names));
}

static string? GetString(JsonElement element, params string[] names)
{
    foreach (var name in names)
    {
        if (TryGetProperty(element, name, out var value))
        {
            return value.GetString();
        }
    }

    return null;
}
