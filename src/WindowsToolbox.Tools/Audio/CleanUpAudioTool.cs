using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class CleanUpAudioTool : BaseTool
{
    public override string Id => "clean-up-audio";
    public override string Name => "Clean Up Audio";
    public override string Description => "Remove background noise from audio recordings";
    public override string Category => "audio";
    public override string Icon => "Audio";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
