using FNLogs.Client.Models;
using FNLogs.Client.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace FNLogs.Client.BackgroundServices;

// TODO: static log messages like we do at work - https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#logging-in-a-non-trivial-app
public class LogUploaderBackgroundService : BackgroundService
{
    private readonly ILogger<LogUploaderBackgroundService> _logger;
    private readonly AppConfig _config;
    private readonly FNLogsApiProvider _provider;

    public LogUploaderBackgroundService(ILogger<LogUploaderBackgroundService> logger, IConfiguration configuration, FNLogsApiProvider provider)
    {
        _logger = logger;
        _config = configuration.Get<AppConfig>() ?? throw new InvalidOperationException("Invalid config");
        _provider = provider;
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
                string dirPath = PathUtils.ReplacePlaceholdersInPath(location.Path);

                if (!Directory.Exists(dirPath))
                {
                    _logger.LogWarning("Skipping location '{path}' for '{process}' because it does not exist", dirPath, location.ProcessName);

                    continue;
                }

                if (Process.GetProcessesByName(location.ProcessName).Length > 0)
                {
                    _logger.LogDebug("Skipping location '{path}' for '{process}' because the process is currently running", dirPath, location.ProcessName);

                    continue;
                }

                DirectoryInfo dir = new(dirPath);
                DirectoryInfo uploadedDir = new(Path.Combine(dirPath, location.UploadedDirectoryName));
                FileInfo[] logFiles = dir.GetFiles($"{location.FileNamePrefix ?? string.Empty}*.log", SearchOption.TopDirectoryOnly);

                if (logFiles.Length == 0)
                {
                    _logger.LogDebug("Location '{path}' has no files, skipping", dirPath);

                    continue;
                }

                string cacheFile = Path.Combine(dirPath, ".fnlogs-cache");
                bool cacheModified = false;
                LocationCache? cache = null;

                if (File.Exists(cacheFile))
                {
                    try
                    {
                        await using FileStream fs = new(cacheFile, FileMode.Open, FileAccess.Read);

                        cache = await JsonSerializer.DeserializeAsync<LocationCache>(fs, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Ignore exception if there is no cache file (e.g. using for the first time)
                        if (ex is not FileNotFoundException)
                        {
                            _logger.LogError("Unexpected exception while reading cache, {type}: {message}", ex.GetType(), ex.Message);
                        }

                    }
                }

                cache ??= new();

                // Ensure uploaded dir exists
                if (!uploadedDir.Exists)
                {
                    uploadedDir.Create();
                }

                foreach (FileInfo logFile in logFiles)
                {
                    try
                    {
                        if (!logFile.Exists)
                        {
                            // just making sure the file still exists when we try to process it
                            continue;
                        }

                        using SHA256 logHash = SHA256.Create();
                        await using FileStream logFileStream = new(logFile.FullName, FileMode.Open, FileAccess.Read);
                        byte[] hashBytes = await logHash.ComputeHashAsync(logFileStream, cancellationToken: stoppingToken);
                        string hash = string.Join(string.Empty, hashBytes.Select(x => x.ToString("x2")));

                        if (cache.KnownHashes.Contains(hash))
                        {
                            _logger.LogDebug("Skipping '{path}' because it is already uploaded", logFile.FullName);

                            continue;
                        }

                        _logger.LogInformation("Uploading '{path}'", logFile.FullName);

                        HttpResponseMessage res = await _provider.UploadLogFileAsync(logFileStream, logFile.Name, stoppingToken);
                        string resContent = res.Content is not null
                            ? await res.Content.ReadAsStringAsync(stoppingToken)
                            : string.Empty;

                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            _logger.LogInformation("Uploaded '{path}' successfully", logFile.FullName);
                        }
                        else
                        {
                            _logger.LogWarning("Uploading '{path}' failed with status {status} {statusText}: {responseText}", logFile.FullName, (int)res.StatusCode, res.ReasonPhrase ?? res.StatusCode.ToString(), resContent);
                        }

                        // dispose stream, so we can move the file
                        await logFileStream.DisposeAsync();

                        // Success or Invalid Log - store result so they are not uploaded again (even if they are moved back)
                        // also move them to the uploaded dir so they dont get deleted by the ue app
                        if (res.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest)
                        {
                            cacheModified = true;
                            cache.KnownHashes.Add(hash);
                        }

                        if (res.StatusCode is HttpStatusCode.OK)
                        {
                            string baseUploadedPath = Path.Combine(uploadedDir.FullName, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Path.GetFileNameWithoutExtension(logFile.Name)}");

                            try
                            {
                                File.Move(logFile.FullName, $"{baseUploadedPath}.log");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Unexpected exception while moving '{path}', {type}: {message}\n{stack}", logFile.FullName, ex.GetType(), ex.Message, ex.StackTrace);

                                continue;
                            }

                            await File.WriteAllTextAsync($"{baseUploadedPath}.json", resContent, stoppingToken);

                            _logger.LogInformation("Moved '{name}' into the uploaded directory at '{uploadedDir}' as '{uploadedName}'", logFile.Name, uploadedDir.FullName, Path.GetFileName(baseUploadedPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Unexpected exception while processing '{path}', {type}: {message}\n{stack}", logFile.FullName, ex.GetType(), ex.Message, ex.StackTrace);
                    }
                }

                if (cacheModified)
                {
                    await File.WriteAllTextAsync(cacheFile, JsonSerializer.Serialize(cache), stoppingToken);

                    _logger.LogDebug("Updated cache for '{path}'", dirPath);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.Interval), stoppingToken);
        }
    }
}
