#if WINDOWS
using System.Diagnostics;

namespace ChatApp.MAUI.Services;

public interface IBackendStartupService
{
    Task EnsureBackendRunningAsync();
}

public class BackendStartupService : IBackendStartupService
{
    private static bool _started;
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to start backend: {ex.Message}");
        }
        await Task.CompletedTask;
    }
}
#endif
