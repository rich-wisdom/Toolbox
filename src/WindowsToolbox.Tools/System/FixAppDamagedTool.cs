using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class FixAppDamagedTool : BaseTool
{
    public override string Id => "fix-app-damaged";
    public override string Name => "Fix \"App Is Damaged\"";
    public override string Description => "Fix SmartScreen and blocked application issues";
    public override string Category => "system";
    public override string Icon => "Shield";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("filePath");
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return Failure("Valid file path is required");
            }

            ReportProgress(progress, 30, "Checking file status...");

            var hasZoneIdentifier = await Task.Run(() => HasZoneIdentifier(filePath), cancellationToken);

            if (hasZoneIdentifier)
            {
                ReportProgress(progress, 60, "Removing Zone.Identifier (Mark of the Web)...");
                var removed = await Task.Run(() => RemoveZoneIdentifier(filePath), cancellationToken);
                
                if (removed)
                {
                    return Success($"Successfully unblocked {Path.GetFileName(filePath)}. The file should now run without SmartScreen warnings.");
                }
                else
                {
                    return Failure("Failed to remove Zone.Identifier. Administrator privileges may be required.");
                }
            }

            ReportProgress(progress, 80, "Unblocking file...");
            var unblocked = await UnblockFile(filePath, cancellationToken);

            if (unblocked)
            {
                return Success($"Successfully unblocked {Path.GetFileName(filePath)}");
            }

            return Success($"File {Path.GetFileName(filePath)} is not blocked or already accessible");
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges are required.");
        }
        catch (Exception ex)
        {
            return Failure($"Error fixing app: {ex.Message}", ex);
        }
    }

    private bool HasZoneIdentifier(string filePath)
    {
        try
        {
            var zoneStream = filePath + ":Zone.Identifier";
            return File.Exists(zoneStream);
        }
        catch
        {
            return false;
        }
    }

    private bool RemoveZoneIdentifier(string filePath)
    {
        try
        {
            var zoneStream = filePath + ":Zone.Identifier";
            if (File.Exists(zoneStream))
            {
                File.Delete(zoneStream);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> UnblockFile(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Unblock-File -Path '{filePath}'\"",
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
