using System.Runtime.InteropServices;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class CleanKeyboardTool : BaseTool
{
    public override string Id => "clean-keyboard";
    public override string Name => "Clean Keyboard";
    public override string Description => "Lock keyboard and mouse for cleaning";
    public override string Category => "system";
    public override string Icon => "Keyboard";

    [DllImport("user32.dll")]
    private static extern bool BlockInput(bool fBlockIt);

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var durationSeconds = parameters.Get<int>("durationSeconds");
            if (durationSeconds == 0) durationSeconds = 30;

            if (durationSeconds > 180)
            {
                return Failure("Maximum duration is 180 seconds (3 minutes) for safety");
            }

            ReportProgress(progress, 10, $"Locking input for {durationSeconds} seconds...");

            BlockInput(true);

            for (int i = 0; i < durationSeconds && !cancellationToken.IsCancellationRequested; i++)
            {
                ReportProgress(progress, 10 + (80 * i / durationSeconds), $"{durationSeconds - i} seconds remaining...");
                await Task.Delay(1000, cancellationToken);
            }

            BlockInput(false);

            ReportProgress(progress, 100, "Input unlocked");

            return Success($"Keyboard cleaning mode completed after {durationSeconds} seconds");
        }
        catch (Exception ex)
        {
            BlockInput(false);
            return Failure($"Error during keyboard lock: {ex.Message}", ex);
        }
    }
}
