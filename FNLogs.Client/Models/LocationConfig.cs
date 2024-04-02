namespace FNLogs.Client.Models;

public class LocationConfig
{
    /// <summary>
    /// Path to the logs directory
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Logs wont be uploaded if a process with this name is running (log is still written)
    /// </summary>
    public required string ProcessName { get; set; }
}
