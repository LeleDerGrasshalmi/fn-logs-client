using FNLogs.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

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

    public async Task<HttpResponseMessage> UploadLogFileAsync(Stream fileStream, string fileName, CancellationToken token)
    {
        // reset position, since we computed the hash before we are at the end
        fileStream.Position = 0;

        using HttpClient client = new();

        MultipartFormDataContent content = new()
        {
            { new StreamContent(fileStream), "file", fileName },
        };

        // TODO: fix "TypeError: Content-Disposition header in FormData part is missing a name." on Cloudflare deployment

        HttpRequestMessage req = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(Path.Combine(_apiHost.ToString(), "api/upload-log")),
            Content = content,
        };

        return await client.SendAsync(req, token);
    }
}
