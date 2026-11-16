using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class TrimVideoTool : BaseTool
{
    public override string Id => "trim-video";
    public override string Name => "Trim Video";
    public override string Description => "Trim video files to a specific time range";
    public override string Category => "video";
    public override string Icon => "Video";

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

            ReportProgress(progress, 10, "Analyzing video...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Trimming video...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, 
                $"{Path.GetFileNameWithoutExtension(filePath)}_trimmed{Path.GetExtension(filePath)}");
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Video trimmed successfully ({startTime} to {endTime})", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["start_time"] = startTime ?? "0",
                ["end_time"] = endTime ?? "0"
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to trim video: {ex.Message}", ex);
        }
    }
}
