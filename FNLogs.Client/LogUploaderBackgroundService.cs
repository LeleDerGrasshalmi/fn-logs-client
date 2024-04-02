using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FNLogs.Client;

public class LogUploaderBackgroundService : BackgroundService
{
    private readonly ILogger<LogUploaderBackgroundService> _logger;

    public LogUploaderBackgroundService(ILogger<LogUploaderBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background service is running at: {time}", DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
