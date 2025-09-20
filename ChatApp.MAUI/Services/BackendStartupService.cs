#if WINDOWS
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ChatApp.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace ChatApp.MAUI.Services;

public interface IBackendStartupService
{
    Task EnsureBackendRunningAsync();
}

public class BackendStartupService : IBackendStartupService
{
    private static bool _started;
    private readonly ILogger<BackendStartupService> _logger;
    private readonly ChatClientOptions _options;

    public BackendStartupService(ILogger<BackendStartupService> logger, IOptions<ChatClientOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task EnsureBackendRunningAsync()
    {
        if (_started) return;
        _started = true;
        try
        {
            // Launch the AppHost which orchestrates backend services
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ..\\DistributedDemo.AppHost\\DistributedDemo.AppHost.csproj",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            Process.Start(psi);
            _logger.LogInformation("Started backend AppHost. Client targeting {BaseUrl}", _options.BaseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start backend");
        }
        await Task.CompletedTask;
    }
}
#endif
