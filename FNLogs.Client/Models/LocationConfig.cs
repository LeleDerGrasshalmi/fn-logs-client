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

    /// <summary>
    /// Prefix for log files, e.g. "FortniteGame" for Fortnite, very much recommended to not upload non UE log files
    /// </summary>
    public string? FileNamePrefix { get; set; }

    /// <summary>
    /// The directory that uploaded logs will be moved to (so they dont get auto deleted by the ue apps)
    /// </summary>
    public string UploadedDirectoryName { get; set; } = "Uploaded";
}
