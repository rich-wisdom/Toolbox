using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class CompressVideoTool : BaseTool
{
    public override string Id => "compress-video";
    public override string Name => "Compress Video";
    public override string Description => "Reduce video file size while maintaining quality";
    public override string Category => "video";
    public override string Icon => "Video";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
