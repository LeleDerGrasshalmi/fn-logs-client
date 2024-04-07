namespace FNLogs.Client.Models;

public class AppConfig
{
    /// <summary>
    /// Locations for logs to watch
    /// </summary>
    public required LocationConfig[] Locations { get; set; }

    /// <summary>
    /// Interval in minutes
    /// </summary>
    public uint Interval { get; set; } = 15;

    /// <summary>
    /// Host of the backend, should only be changed in the config for debugging purposes
    /// </summary>
    public required Uri ApiHost { get; set; }
}
