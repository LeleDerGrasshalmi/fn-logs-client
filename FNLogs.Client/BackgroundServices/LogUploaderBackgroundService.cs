using FNLogs.Client.Models;
using FNLogs.Client.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FNLogs.Client.BackgroundServices;

public class LogUploaderBackgroundService : BackgroundService
{
    private readonly ILogger<LogUploaderBackgroundService> _logger;
    private readonly AppConfig _config;

    public LogUploaderBackgroundService(ILogger<LogUploaderBackgroundService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration.Get<AppConfig>() ?? throw new InvalidOperationException("Invalid config");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_config.Locations.Length == 0)
        {
            _logger.LogWarning("Exciting worker because no locations have been configured");

            Environment.Exit(0);
        }

        _logger.LogInformation("Starting worker with interval {interval}", _config.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (LocationConfig location in _config.Locations)
            {
                string dir = PathUtils.ReplacePlaceholdersInPath(location.Path);

                if (!Directory.Exists(dir))
                {
                    _logger.LogWarning("Skipping location '{path}' for '{process}' because it does not exist", dir, location.ProcessName);

                    continue;
                }

                if (Process.GetProcessesByName(location.ProcessName).Length > 0)
                {
                    _logger.LogWarning("Skipping location '{path}' for '{process}' because the process is currently running", dir, location.ProcessName);

                    continue;
                }

                string cacheFile = Path.Combine(dir, ".fnlogs-cache");

                // TODO: read cache, determine non uploaded files, upload those, store result (hash)
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.Interval), stoppingToken);
        }
    }
}
