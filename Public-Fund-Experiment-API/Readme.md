# Public Fund Experiment API

## Setup

- Create `appsettings.Development.json` under the project with the following format:

```
{
  "GitHub": {
    "Token": "[Your GitHub token]"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- dotnet restore

- dotnet run
