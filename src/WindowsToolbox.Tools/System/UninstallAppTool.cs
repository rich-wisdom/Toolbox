using System.Diagnostics;
using Microsoft.Win32;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class UninstallAppTool : BaseTool
{
    public override string Id => "uninstall-app";
    public override string Name => "Uninstall App";
    public override string Description => "Uninstall applications with deep cleanup";
    public override string Category => "system";
    public override string Icon => "Delete";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var listOnly = parameters.Get<bool>("listOnly");
            
            if (listOnly)
            {
                ReportProgress(progress, 50, "Scanning installed applications...");
                var apps = await Task.Run(() => GetInstalledApplications(), cancellationToken);
                return Success($"Found {apps.Count} installed applications", new Dictionary<string, object> { ["applications"] = apps });
            }

            var appName = parameters.Get<string>("appName");
            if (string.IsNullOrEmpty(appName))
            {
                return Failure("Application name is required for uninstallation");
            }

            ReportProgress(progress, 30, $"Uninstalling {appName}...");

            var success = await UninstallApplication(appName, cancellationToken);
            
            if (!success)
            {
                return Failure($"Failed to uninstall {appName}. The application may not be found or require administrator privileges.");
            }

            ReportProgress(progress, 100, "Uninstall complete");

            return Success($"Successfully uninstalled {appName}");
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges are required for uninstalling applications.");
        }
        catch (Exception ex)
        {
            return Failure($"Error during uninstallation: {ex.Message}", ex);
        }
    }

    private List<object> GetInstalledApplications()
    {
        var apps = new List<object>();
        var registryPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var path in registryPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var displayName = subKey.GetValue("DisplayName") as string;
                            if (!string.IsNullOrEmpty(displayName))
                            {
                                var size = subKey.GetValue("EstimatedSize");
                                var installDate = subKey.GetValue("InstallDate") as string;
                                var publisher = subKey.GetValue("Publisher") as string;

                                apps.Add(new
                                {
                                    name = displayName,
                                    publisher = publisher ?? "Unknown",
                                    version = subKey.GetValue("DisplayVersion") as string ?? "",
                                    installDate = installDate ?? "Unknown",
                                    sizeKB = size != null ? Convert.ToInt64(size) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch { }
        }

        return apps.OrderBy(a => ((dynamic)a).name).ToList();
    }

    private async Task<bool> UninstallApplication(string appName, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = $"product where name=\"{appName}\" call uninstall /nointeractive",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync(cancellationToken);
                return process.ExitCode == 0;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
