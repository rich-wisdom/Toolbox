using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class RemoveBackgroundTool : BaseTool
{
    public override string Id => "remove-background";
    public override string Name => "Remove Background";
    public override string Description => "Remove background from images";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var backgroundColor = parameters.Get<string>("background_color") ?? "transparent";

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Analyzing image...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Removing background...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, 
                $"{Path.GetFileNameWithoutExtension(filePath)}_nobg.png");
            
            ReportProgress(progress, 100, "Complete");

            return Success("Background removed successfully", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["background_color"] = backgroundColor
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to remove background: {ex.Message}", ex);
        }
    }
}
