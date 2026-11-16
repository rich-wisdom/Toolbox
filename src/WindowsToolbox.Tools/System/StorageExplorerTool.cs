using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class StorageExplorerTool : BaseTool
{
    public override string Id => "storage-explorer";
    public override string Name => "Storage Explorer";
    public override string Description => "Analyze disk usage and find large files";
    public override string Category => "system";
    public override string Icon => "HardDrive";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var drivePath = parameters.Get<string>("drivePath") ?? "C:\\";
            var minSizeMb = parameters.Get<int>("minSizeMb");
            if (minSizeMb == 0) minSizeMb = 100;

            ReportProgress(progress, 10, "Scanning drives...");

            var driveInfo = new DriveInfo(Path.GetPathRoot(drivePath) ?? "C:\\");
            if (!driveInfo.IsReady)
            {
                return Failure("Drive is not ready or accessible");
            }

            ReportProgress(progress, 30, "Analyzing disk usage...");

            var largeFiles = new List<FileInfo>();
            await Task.Run(() => ScanDirectory(new DirectoryInfo(drivePath), minSizeMb, largeFiles, cancellationToken), cancellationToken);

            ReportProgress(progress, 90, "Generating report...");

            var result = new Dictionary<string, object>
            {
                ["totalSize"] = driveInfo.TotalSize,
                ["freeSpace"] = driveInfo.AvailableFreeSpace,
                ["usedSpace"] = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                ["largeFiles"] = largeFiles.Select(f => new
                {
                    path = f.FullName,
                    size = f.Length,
                    lastModified = f.LastWriteTime
                }).ToList()
            };

            return Success($"Found {largeFiles.Count} large files (>{minSizeMb}MB)", result);
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges may be required.");
        }
        catch (Exception ex)
        {
            return Failure($"Error analyzing storage: {ex.Message}", ex);
        }
    }

    private void ScanDirectory(DirectoryInfo directory, int minSizeMb, List<FileInfo> largeFiles, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        try
        {
            foreach (var file in directory.GetFiles())
            {
                if (cancellationToken.IsCancellationRequested) return;
                if (file.Length > minSizeMb * 1024 * 1024)
                {
                    largeFiles.Add(file);
                }
            }

            foreach (var subDir in directory.GetDirectories())
            {
                if (cancellationToken.IsCancellationRequested) return;
                if (subDir.Name == "System Volume Information" || subDir.Name == "$Recycle.Bin")
                    continue;
                ScanDirectory(subDir, minSizeMb, largeFiles, cancellationToken);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (Exception) { }
    }
}
