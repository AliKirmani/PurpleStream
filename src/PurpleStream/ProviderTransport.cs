using System.Net;
using System.Net.Sockets;

namespace PurpleStream;

sealed class ProviderFailure(string message) : Exception(message);

sealed class ProviderTransport : IDisposable
{
    readonly HttpClient client;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(3);

    public ProviderTransport(HttpMessageHandler? handler = null)
    {
        client = new HttpClient(handler ?? new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        }) { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PurpleStream/0.2");
    }

    public async Task<string> GetText(string url, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(RequestTimeout);
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode)
            {
                var code = (int)response.StatusCode;
                throw new ProviderFailure(code is 401 or 403
                    ? $"Access was rejected (HTTP {code}). Check your account and provider restrictions."
                    : $"The provider returned HTTP {code}. Check the server URL or try M3U fallback.");
            }
            // The deadline covers both headers and the complete response body.
            return await response.Content.ReadAsStringAsync(deadline.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The provider did not respond within three minutes. Retry or try M3U fallback.");
        }
    }

    public static string BaseServer(string value)
    {
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ProviderFailure("Enter the full server URL including http:// or https://.");
        var builder = new UriBuilder(uri) { Query = "", Fragment = "" };
        var path = builder.Path.TrimEnd('/');
        if (new[] { "/get.php", "/player_api.php", "/app.php", "/xmltv.php" }
            .Any(endpoint => path.EndsWith(endpoint, StringComparison.OrdinalIgnoreCase)))
            path = path[..path.LastIndexOf('/')];
        builder.Path = path;
        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }

    public static string ErrorText(Exception error)
    {
        if(error is ProviderFailure or TimeoutException)return error.Message;
        if(error is System.Text.Json.JsonException)return "The provider returned an unexpected response. Check the server or try M3U fallback.";
        if(error is HttpRequestException)return "Network connection failed. Check the server, port, internet connection and provider availability.";
        if(error is IOException or UnauthorizedAccessException)return "A file could not be read or saved. Check disk space, folder access and file availability.";
        return "This operation could not complete. Check your provider connection and retry.";
    }
    public void Dispose() => client.Dispose();
}

static class NetworkTests
{
    sealed class Fake(Func<CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => respond(token);
    }
    public static async Task Run()
    {
        using var slow = new ProviderTransport(new Fake(async token =>
        {
            await Task.Delay(1000, token);
            return new(HttpStatusCode.OK) { Content = new StringContent("ok") };
        })) { RequestTimeout = TimeSpan.FromMilliseconds(80) };
        try { await slow.GetText("https://fixture.invalid", default); throw new Exception("Missing timeout"); }
        catch (TimeoutException) { }
        using var cancel = new CancellationTokenSource(20);
        try { await slow.GetText("https://fixture.invalid", cancel.Token); throw new Exception("Missing cancellation"); }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        using var denied = new ProviderTransport(new Fake(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden))));
        try { await denied.GetText("https://fixture.invalid?password=PRIVATE", default); throw new Exception("Missing HTTP error"); }
        catch (ProviderFailure ex) { if (!ex.Message.Contains("403") || ex.Message.Contains("PRIVATE")) throw; }
        if (ProviderTransport.BaseServer("http://example.com:8880/app.php?secret=PRIVATE") != "http://example.com:8880")
            throw new Exception("Server normalization failed");
        if (ProviderTransport.BaseServer("https://example.com/panel/player_api.php") != "https://example.com/panel")
            throw new Exception("Subpath normalization failed");
        var safe = ProviderTransport.ErrorText(new HttpRequestException("https://example.com?password=PRIVATE"));
        if (safe.Contains("PRIVATE")) throw new Exception("Credential leak");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "network-test.txt"),
            "PASS: timeout distinct from user cancellation; HTTP status preserved without credentials; server endpoint/subpath normalization; network errors redact URLs. No provider contacted.");
    }
}

