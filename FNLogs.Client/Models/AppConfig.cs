namespace FNLogs.Client.Models;

public class AppConfig
{
    /// <summary>
    /// Locations for logs to watch
    /// </summary>
    public LocationConfig[] Locations { get; set; } = [];

    /// <summary>
    /// Interval in minutes
    /// </summary>
    public uint Interval { get; set; } = 15;
}
