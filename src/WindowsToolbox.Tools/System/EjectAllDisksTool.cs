using System.Diagnostics;
using System.Management;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class EjectAllDisksTool : BaseTool
{
    public override string Id => "eject-all-disks";
    public override string Name => "Eject All Disks";
    public override string Description => "Safely remove all removable drives";
    public override string Category => "system";
    public override string Icon => "Eject";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var listOnly = parameters.Get<bool>("listOnly");

            ReportProgress(progress, 20, "Scanning for removable drives...");

            var removableDrives = await Task.Run(() => GetRemovableDrives(), cancellationToken);

            if (removableDrives.Count == 0)
            {
                return Success("No removable drives found");
            }

            if (listOnly)
            {
                return Success($"Found {removableDrives.Count} removable drives", 
                    new Dictionary<string, object> { ["drives"] = removableDrives });
            }

            ReportProgress(progress, 50, "Ejecting drives...");

            var ejectedCount = 0;
            var failedDrives = new List<string>();

            foreach (var drive in removableDrives)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var success = await EjectDrive(drive.ToString() ?? "", cancellationToken);
                if (success)
                    ejectedCount++;
                else
                    failedDrives.Add(drive.ToString() ?? "");
            }

            ReportProgress(progress, 100, "Complete");

            var result = new Dictionary<string, object>
            {
                ["ejectedCount"] = ejectedCount,
                ["failedDrives"] = failedDrives
            };

            if (failedDrives.Count > 0)
            {
                return Success($"Ejected {ejectedCount} of {removableDrives.Count} drives. {failedDrives.Count} failed.", result);
            }

            return Success($"Successfully ejected all {ejectedCount} removable drives", result);
        }
        catch (Exception ex)
        {
            return Failure($"Error ejecting drives: {ex.Message}", ex);
        }
    }

    private List<object> GetRemovableDrives()
    {
        var drives = new List<object>();

        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    drives.Add(new
                    {
                        driveLetter = drive.Name,
                        volumeLabel = drive.VolumeLabel,
                        totalSize = drive.TotalSize,
                        freeSpace = drive.AvailableFreeSpace
                    });
                }
            }
        }
        catch { }

        return drives;
    }

    private async Task<bool> EjectDrive(string driveLetter, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"$driveEject = New-Object -comObject Shell.Application; $driveEject.Namespace(17).ParseName('{driveLetter}').InvokeVerb('Eject')\"",
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
