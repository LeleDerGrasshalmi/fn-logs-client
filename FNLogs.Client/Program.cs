using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FNLogs.Client;

internal class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = new HostBuilder()
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {
                loggingBuilder.AddConsole();
            })
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<LogUploaderBackgroundService>();
            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
}