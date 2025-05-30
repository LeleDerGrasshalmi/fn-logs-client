# FNLogs - Client

This is the client for [FNLogs](https://github.com/LeleDerGrasshalmi/fn-logs), which automaticly uploads log files based on the configuration.

## Getting Started

1. Download the built app from the latest github action run.
2. Extract the zip content
3. Start `FNLogs.Client.exe`
4. (ideally) Create a shortcup for the exe in `%appdata%\Microsoft\Windows\Start Menu\Programs\Startup` so the app auto-starts on boot

## Requirements

### User

- DotNet 8 Runtime

### Development

- DotNet 8 SDK

---

https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Build

```bash
dotnet build
```

## App Logs

App Logs are located at `%localappdata%\FNLogs\Logs`. The app uses Serilog for logging.

## App Configuration

The app is configured in `appsettings.json`, when debugging the app will always use the latest version in the project. <br/>
When using a release build your config will be located at `%localappdata%\FNLogs\appsettings.json`.

### Config Format

| Path                              | Required | Type             | Description                                                                                                    |
| --------------------------------- | -------- | ---------------- | -------------------------------------------------------------------------------------------------------------- |
| `apiHost`                         | true     | string           | This is the backend host, for the hosted version use `https://fnlogs.lel3x.de`, which is also the default host |
| `interval`                        | false    | uint             | The interval **in minutes** in which logs are validated and uploaded, defaults to 30 minutes                   |
| `locations`                       | true     | LocationConfig[] | This is the list of log directories to watch                                                                   |
| `locations/path`                  | true     | string           | The directory of the logs, you may use the `%localappdata%` variable                                           |
| `locations/processName`           | true     | string           | Logs wont be uploaded while this process is running                                                            |
| `locations/fileNamePrefix`        | false    | string           | Prefix for log files, e.g. "FortniteGame" for Fortnite, very much recommended to not upload non UE log files   |
| `locations/uploadedDirectoryName` | false    | string           | The directory that uploaded logs will be moved to, defaults to `Uploaded`                                      |

Example Configuration (only watch fortnite logs)

```js
{
  "apiHost": "https://fnlogs.lel3x.de",
  "locations": [
    {
      "path": "%localappdata%\\FortniteGame\\Saved\\Logs",
      "processName": "FortniteClient-Win64-Shipping",
      "fileNamePrefix": "FortniteGame",
      "uploadedDirectoryName": "Uploaded"
    }
  ],
  "interval": 30
}
```

---

The default shipped configuration will watch for both fortnite and epic games launcher logs.

## Issues / Bugs

Found a issue?

Open a [GitHub issue](https://github.com/LeleDerGrasshalmi/fn-logs-client/issues) with enough info to reproduce the issue.
