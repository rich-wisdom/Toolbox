using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class ResetPermissionsTool : BaseTool
{
    public override string Id => "reset-permissions";
    public override string Name => "Reset Permissions";
    public override string Description => "Reset file/folder permissions to defaults";
    public override string Category => "system";
    public override string Icon => "Permissions";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var path = parameters.Get<string>("path");
            if (string.IsNullOrEmpty(path) || (!File.Exists(path) && !Directory.Exists(path)))
            {
                return Failure("Valid file or folder path is required");
            }

            var action = parameters.Get<string>("action") ?? "take-ownership";
            var recursive = parameters.Get<bool>("recursive");

            switch (action.ToLower())
            {
                case "take-ownership":
                    ReportProgress(progress, 30, "Taking ownership...");
                    var owned = await TakeOwnership(path, recursive, cancellationToken);
                    return owned 
                        ? Success($"Successfully took ownership of {Path.GetFileName(path)}")
                        : Failure("Failed to take ownership. Administrator privileges are required.");

                case "grant-full-control":
                    ReportProgress(progress, 30, "Granting full control...");
                    var granted = await GrantFullControl(path, recursive, cancellationToken);
                    return granted 
                        ? Success($"Successfully granted full control to current user for {Path.GetFileName(path)}")
                        : Failure("Failed to grant permissions. Administrator privileges are required.");

                case "reset-to-default":
                    ReportProgress(progress, 30, "Resetting permissions...");
                    var reset = await ResetToDefault(path, recursive, cancellationToken);
                    return reset 
                        ? Success($"Successfully reset permissions for {Path.GetFileName(path)}")
                        : Failure("Failed to reset permissions. Administrator privileges are required.");

                default:
                    return Failure($"Unknown action: {action}");
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges are required.");
        }
        catch (Exception ex)
        {
            return Failure($"Error resetting permissions: {ex.Message}", ex);
        }
    }

    private async Task<bool> TakeOwnership(string path, bool recursive, CancellationToken cancellationToken)
    {
        try
        {
            var recursiveFlag = recursive ? "/R" : "";
            var psi = new ProcessStartInfo
            {
                FileName = "takeown",
                Arguments = $"/F \"{path}\" {recursiveFlag}",
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

    private async Task<bool> GrantFullControl(string path, bool recursive, CancellationToken cancellationToken)
    {
        try
        {
            var username = Environment.UserName;
            var recursiveFlag = recursive ? "/T" : "";
            var psi = new ProcessStartInfo
            {
                FileName = "icacls",
                Arguments = $"\"{path}\" /grant {username}:F {recursiveFlag}",
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

    private async Task<bool> ResetToDefault(string path, bool recursive, CancellationToken cancellationToken)
    {
        try
        {
            var recursiveFlag = recursive ? "/T" : "";
            var psi = new ProcessStartInfo
            {
                FileName = "icacls",
                Arguments = $"\"{path}\" /reset {recursiveFlag}",
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
