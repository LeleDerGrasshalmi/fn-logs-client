using FNLogs.Client.BackgroundServices;
using FNLogs.Client.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FNLogs.Client;

internal class Program
{
    public const string AppSettingsName = "appsettings.json";
    public static string LocalAppSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FNLogs", AppSettingsName);
    public static string BaseAppSettings = Path.Combine(Environment.CurrentDirectory, AppSettingsName);

    public static async Task Main(string[] args)
    {
        IHost host = new HostBuilder()
            .ConfigureAppConfiguration((configBuilder) =>
            {
#if !DEBUG
                if (!File.Exists(LocalAppSettings))
                {
                    string? dir = Path.GetDirectoryName(LocalAppSettings);

                    if (dir is not null && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.Copy(BaseAppSettings, LocalAppSettings);
                }

                configBuilder.AddJsonFile(LocalAppSettings);
#else
                configBuilder.AddJsonFile(BaseAppSettings);
#endif
            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {
#if DEBUG
                LogLevel level = LogLevel.Trace;
#else
                LogLevel level = LogLevel.Information;
#endif

                loggingBuilder
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