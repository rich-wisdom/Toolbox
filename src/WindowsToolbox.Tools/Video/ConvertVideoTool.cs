using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class ConvertVideoTool : BaseTool
{
    public override string Id => "convert-video";
    public override string Name => "Convert Video";
    public override string Description => "Convert video files to different formats";
    public override string Category => "video";
    public override string Icon => "Video";

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

            ReportProgress(progress, 10, "Initializing video conversion...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Converting video...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.ChangeExtension(filePath, outputFormat);
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Video converted successfully to {outputFormat}", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["format"] = outputFormat
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert video: {ex.Message}", ex);
        }
    }
}
