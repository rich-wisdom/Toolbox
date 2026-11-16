using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class ScreenshotTool : BaseTool
{
    public override string Id => "screenshot";
    public override string Name => "Screenshot";
    public override string Description => "Capture screen, window, or region";
    public override string Category => "image";
    public override string Icon => "Camera";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
