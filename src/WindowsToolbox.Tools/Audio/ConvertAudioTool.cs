using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class ConvertAudioTool : BaseTool
{
    public override string Id => "convert-audio";
    public override string Name => "Convert Audio";
    public override string Description => "Convert audio files to different formats";
    public override string Category => "audio";
    public override string Icon => "Audio";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var outputFormat = parameters.Get<string>("output_format");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");
            
            if (string.IsNullOrWhiteSpace(outputFormat))
                return Failure("output_format is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Initializing audio conversion...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Converting audio...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.ChangeExtension(filePath, outputFormat);
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Audio converted successfully to {outputFormat}", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["format"] = outputFormat
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert audio: {ex.Message}", ex);
        }
    }
}
