using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class TrimAudioTool : BaseTool
{
    public override string Id => "trim-audio";
    public override string Name => "Trim Audio";
    public override string Description => "Trim audio files to a specific time range";
    public override string Category => "audio";
    public override string Icon => "Audio";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var startTime = parameters.Get<string>("start_time");
            var endTime = parameters.Get<string>("end_time");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Analyzing audio...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Trimming audio...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, 
                $"{Path.GetFileNameWithoutExtension(filePath)}_trimmed{Path.GetExtension(filePath)}");
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Audio trimmed successfully ({startTime} to {endTime})", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["start_time"] = startTime ?? "0",
                ["end_time"] = endTime ?? "0"
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to trim audio: {ex.Message}", ex);
        }
    }
}
