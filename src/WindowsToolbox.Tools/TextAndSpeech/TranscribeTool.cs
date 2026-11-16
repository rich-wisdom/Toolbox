using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class TranscribeTool : BaseTool
{
    public override string Id => "transcribe";
    public override string Name => "Transcribe";
    public override string Description => "Convert speech to text from audio or video";
    public override string Category => "text-speech";
    public override string Icon => "Microphone";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
