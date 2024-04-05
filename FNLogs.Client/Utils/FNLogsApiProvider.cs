using FNLogs.Client.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FNLogs.Client.Utils;

public class FNLogsApiProvider
{
    private readonly Uri _apiHost;
    private readonly ILogger<FNLogsApiProvider> _logger;

    public FNLogsApiProvider(IConfiguration configuration, ILogger<FNLogsApiProvider> logger)
    {
        _apiHost = configuration.Get<AppConfig>()!.ApiHost;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> UploadLogFileAsync(Stream stream, string name, CancellationToken token)
    {
        // reset position, since we computed the hash before we are at the end
        stream.Position = 0;

        using HttpClient client = new();

        StreamContent fileContent = new(stream);
        MultipartFormDataContent content = new()
        {
            { fileContent, "\"file\"", $"\"{name}\"" },
        };

        // define that its plain text
        fileContent.Headers.ContentType = new("text/plain");

        HttpRequestMessage req = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(Path.Combine(_apiHost.ToString(), "api", "upload-log")),
            Content = content,
        };

        // json response
        req.Headers.Accept.Add(new("application/json"));

        return await client.SendAsync(req, token);
    }
}
