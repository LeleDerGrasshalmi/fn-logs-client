namespace FNLogs.Client.Models;

public class LocationCache
{
    /// <summary>
    /// Known file hashes that are already uploaded
    /// </summary>
    public List<string> KnownHashes { get; set; } = [];
}
