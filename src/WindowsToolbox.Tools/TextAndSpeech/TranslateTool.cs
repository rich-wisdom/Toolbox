using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class TranslateTool : BaseTool
{
    public override string Id => "translate";
    public override string Name => "Translate";
    public override string Description => "Translate text between languages";
    public override string Category => "text-speech";
    public override string Icon => "LocalLanguage";
    public override bool IsAvailable => false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure(
            "Translation is not implemented. No translated text or file was produced."));
    }
}
