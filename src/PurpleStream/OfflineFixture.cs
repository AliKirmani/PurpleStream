using System.Net;

namespace PurpleStream;

// A deterministic in-memory provider for regression checks in environments
// where outbound and loopback networking are unavailable. Never used normally.
sealed class OfflineFixture : HttpMessageHandler
{
    readonly Dictionary<string, int> counts = new();
    bool failVod;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var uri = request.RequestUri!;
        var query = uri.Query.TrimStart('?').Split('&').Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => p.Length > 1 ? Uri.UnescapeDataString(p[1]) : "");
        string text;
        var status = HttpStatusCode.OK;
        if (uri.AbsolutePath == "/control")
        {
            if (query.ContainsKey("reset")) counts.Clear();
            if (query.TryGetValue("failVod", out var flag)) failVod = flag == "1";
            text = "ok";
        }
        else if (uri.AbsolutePath == "/stats") text = System.Text.Json.JsonSerializer.Serialize(counts);
        else if (uri.AbsolutePath is "/list.m3u" or "/get.php")
            text = "#EXTM3U\n#EXTINF:-1 group-title=\"Test\",First\nhttp://127.0.0.1:18880/video.avi\n#EXTINF:-1,Second\nhttp://127.0.0.1:18880/video.avi";
        else
        {
            var action = query.GetValueOrDefault("action", "auth");
            counts[action] = counts.GetValueOrDefault(action) + 1;
            if (query.GetValueOrDefault("username") != "test" || query.GetValueOrDefault("password") != "fixture")
                text = "{\"user_info\":{\"auth\":0}}";
            else if (action == "get_vod_streams" && failVod)
            {
                status = HttpStatusCode.ServiceUnavailable;
                text = "unavailable";
            }
            else text = action switch
            {
                "get_live_categories" => "[{\"category_id\":1,\"category_name\":\"Pakistan | Sports\"}]",
                "get_live_streams" => "[{\"stream_id\":1,\"name\":\"Fixture live\",\"category_id\":1,\"tv_archive\":1,\"tv_archive_duration\":7}]",
                "get_vod_categories" => "[{\"category_id\":2,\"category_name\":\"India | Action\"}]",
                "get_series_categories" => "[{\"category_id\":3,\"category_name\":\"Turkey | Drama\"}]",
                "get_vod_streams" => "[{\"stream_id\":2,\"category_id\":2,\"name\":\"Fixture movie\",\"container_extension\":\"avi\"}]",
                "get_series" => "[{\"series_id\":3,\"category_id\":3,\"name\":\"Fixture series\"}]",
                "get_simple_data_table" => System.Text.Json.JsonSerializer.Serialize(new {epg_listings=new[]{new {title=Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Fixture programme")),description=Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Fixture description")),start_timestamp=DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds().ToString(),stop_timestamp=DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds().ToString()}}}),
                "get_series_info" => "{\"episodes\":{\"1\":[{\"id\":4,\"title\":\"Test episode\",\"container_extension\":\"avi\"}]}}",
                _ => "{\"user_info\":{\"auth\":1,\"status\":\"Active\",\"exp_date\":\"1893456000\"}}"
            };
        }
        return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(text) });
    }
}

