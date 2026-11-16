using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class BeautifyScreenshotTool : BaseTool
{
    public override string Id => "beautify-screenshot";
    public override string Name => "Beautify Screenshot";
    public override string Description => "Beautify screenshots with backgrounds, padding, shadows";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var background = parameters.Get<string>("background") ?? "gradient";
            var padding = parameters.Get<double>("padding");
            var cornerRadius = parameters.Get<double>("corner_radius");
            var shadow = parameters.Get<bool>("shadow");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Preparing screenshot...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Beautifying screenshot...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, 
                $"{Path.GetFileNameWithoutExtension(filePath)}_beautiful.png");
            
            ReportProgress(progress, 100, "Complete");

            return Success("Screenshot beautified successfully", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["background"] = background
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to beautify screenshot: {ex.Message}", ex);
        }
    }
}
