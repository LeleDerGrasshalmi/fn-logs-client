using FNLogs.Client.BackgroundServices;
using FNLogs.Client.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

namespace FNLogs.Client;

internal class Program
{
    public const string AppSettingsName = "appsettings.json";
    public static string LocalAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FNLogs");
    public static string LogsPath = Path.Combine(LocalAppDataPath, "Logs");
    public static string LocalAppSettings = Path.Combine(LocalAppDataPath, AppSettingsName);
    public static string BaseAppSettings = Path.Combine(Environment.CurrentDirectory, AppSettingsName);

    public static async Task Main(string[] args)
    {
        if (LogsPath is not null
            && !Directory.Exists(LogsPath))
        {
            Directory.CreateDirectory(LogsPath);
        }

        IHost host = new HostBuilder()
            .ConfigureAppConfiguration((configBuilder) =>
            {
#if !DEBUG
                if (!File.Exists(LocalAppSettings))
                {
                    File.Copy(BaseAppSettings, LocalAppSettings);
                }

                configBuilder.AddJsonFile(LocalAppSettings);
#else
                configBuilder.AddJsonFile(BaseAppSettings);
#endif
            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {
                LogLevel level = LogLevel.Information;
                LoggerConfiguration config = new LoggerConfiguration()
                    .WriteTo.File($"{LogsPath}/fn-logs-client.log", rollingInterval: RollingInterval.Day);

#if DEBUG
                config.MinimumLevel.Verbose();
                level = LogLevel.Trace;
#else
                config.MinimumLevel.Information();
#endif
                Log.Logger = config.CreateLogger();

                loggingBuilder
                    .AddSerilog(dispose: true)
                    .AddConsole()
                    .SetMinimumLevel(level);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<FNLogsApiProvider>();
                services.AddHostedService<LogUploaderBackgroundService>();
            })
            .Build();

        await host.RunAsync();
    }
}